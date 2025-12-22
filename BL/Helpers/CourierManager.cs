using DalApi;
using System.Text.RegularExpressions;


namespace Helpers;

/// <summary>
/// Manages courier-related operations.
/// </summary>
/// <remarks>
/// internal class to restrict access to BL layer only.
/// </remarks>
internal static class CourierManager
{
    //an object for accessing the DAL methods
    private static IDal s_dal = Factory.Get;

    //an observer manager for courier-related changes
    internal static ObserverManager Observers = new();


    /// <summary>
    /// creates a new courier after validating input and checking for duplicates.
    /// </summary>
    /// <param name="courier"></param>
    /// <exception cref="BO.BlAlreadyExistsException"></exception>
    internal static void CreateCourier(BO.Courier courier)
    {
        // Validate input by calling the helper method
        ValidateCourierData(courier);

        string hashedPassword = Tools.HashPassword(courier.Password!);

        // Check for duplicate ID (InvalidOperationException)
        try
        {
            if (s_dal.Courier.Read(courier.Id) != null)
                throw new BO.BlAlreadyExistsException($"Courier with ID {courier.Id} already exists in the system.");
        }
        catch (DO.DalDoesNotExistException)
        {
            // Expected path if courier does not exist
        }

        // Mapping BO to DO and creating the courier
        DO.Courier doCourier = new DO.Courier
        (
            Id: courier.Id,
            Name: courier.Name!,
            Phone: courier.Phone!,
            Email: courier.Email!,
            Password: hashedPassword,
            IsActive: true,
            TypeOfDelivery: (DO.DeliveryType)courier.TypeOfDelivery,
            StartWorkTime: AdminManager.Now,
            MaxDistance: courier.MaxDistance
        );
        s_dal.Courier.Create(doCourier);

        // Notify observers about the new courier
        Observers.NotifyListUpdated();
    }

    /// <summary>
    /// reads all couriers from the DAL, maps them to BO, and attaches statistical data.
    /// </summary>
    /// <param name="isActive"></param>
    /// <returns></returns>
    internal static IEnumerable<BO.CourierInList> ReadAllCouriers(bool? isActive = null)
    {
        // filter by isActive if provided
        var dalCouriers = s_dal.Courier.ReadAll(isActive.HasValue ? d => d.IsActive == isActive.Value : null);

        // Mapping to BO.CourierInList entities
        return dalCouriers.Select(doCourier =>
        {
            // Calculate statistics
            var stats = GetCourierStatistics(doCourier.Id);

            DO.Delivery? openDelivery = FindOpenDeliveryForCourier(doCourier.Id);
            int? currentOrderId = openDelivery?.OrderId;

            return new BO.CourierInList
            {
                Id = doCourier.Id,
                Name = doCourier.Name,
                IsActive = doCourier.IsActive,
                TypeOfDelivery = (BO.DeliveryType)doCourier.TypeOfDelivery,
                StartWorkTime = doCourier.StartWorkTime,
                TotalOnTimeDeliveries = stats.DeliveredOnTime,
                TotalLateDeliveries = stats.DeliveredLate,
                CurrentOrderId = currentOrderId
            };
        });
    }

    /// <summary>
    /// reads a specific courier by ID, including statistics and current order if any.
    /// </summary>
    /// <param name="courierId"></param>
    /// <returns></returns>
    internal static BO.Courier ReadCourier(int courierId)
    {
        // Retrieve existing courier or throw if not found by calling helper method
        DO.Courier doCourier = GetExistingCourier(courierId);

        // Calculate statistics
        var stats = GetCourierStatistics(courierId);

        // Find open delivery (CurrentOrder)
        BO.OrderInProgress? currentOrder = null;
        DO.Delivery? openDelivery = FindOpenDeliveryForCourier(courierId);

        if (openDelivery != null)
            currentOrder = MapOpenDeliveryToOrderInProgress(openDelivery);

        // Mapping to BO
        return new BO.Courier
        {
            Id = doCourier.Id,
            Name = doCourier.Name,
            Phone = doCourier.Phone,
            Email = doCourier.Email,
            IsActive = doCourier.IsActive,
            MaxDistance = doCourier.MaxDistance,
            TypeOfDelivery = (BO.DeliveryType)doCourier.TypeOfDelivery,
            StartWorkTime = doCourier.StartWorkTime,
            TotalOnTimeDeliveries = stats.DeliveredOnTime,
            TotalLateDeliveries = stats.DeliveredLate,
            CurrentOrder = currentOrder
        };
    }

    /// <summary>
    /// updates an existing courier's information.
    /// </summary>
    /// <param name="courier"></param>
    internal static void UpdateCourier(BO.Courier courier)
    {
        // retrieve existing courier or throw if not found by calling helper method
        DO.Courier existingCourier = GetExistingCourier(courierId: courier.Id);

        // determine password to store
        string passwordToStore = string.IsNullOrEmpty(courier.Password) ? existingCourier.Password : Tools.HashPassword(courier.Password);

        // map updated fields
        DO.Courier updatedCourier = existingCourier with
        {
            Name = courier.Name!,
            Phone = courier.Phone!,
            Email = courier.Email!,
            Password = passwordToStore,
            TypeOfDelivery = (DO.DeliveryType)courier.TypeOfDelivery,
            MaxDistance = courier.MaxDistance
        };

        // perform the update
        s_dal.Courier.Update(updatedCourier);

        Observers.NotifyItemUpdated(courier.Id);
        Observers.NotifyListUpdated();
    }

    /// <summary>
    /// deletes a courier if they have no associated deliveries.
    /// </summary>
    /// <param name="courierId"></param>
    /// <exception cref="InvalidOperationException"></exception>
    internal static void DeleteCourier(int courierId)
    {
        // check for existing deliveries
        if (s_dal.Delivery.ReadAll(d => d.CourierId == courierId).Any())
            throw new BO.BlCannotDeleteException($"Cannot delete Courier {courierId}: courier is associated with existing deliveries (history).");

        // perform deletion
        try
        {
            s_dal.Courier.Delete(courierId);
        }
        catch (DO.DalDoesNotExistException)
        {
            throw;
        }

        // notify observers about the deletion
        Observers.NotifyListUpdated();
    }

    /// <summary>
    /// gets the last activity time of a courier based on their deliveries.
    /// </summary>
    /// <param name="courierId"></param>
    /// <returns></returns>
    internal static DateTime? GetLastActivityTime(int courierId)
    {
        // assume using System.Linq; is present
        var courierDeliveries = s_dal.Delivery.ReadAll(d => d.CourierId == courierId);
        var activityTimes = courierDeliveries
            .SelectMany(d => new[] { d.DeliveryStartTime, d.DeliveryEndTime })
            .Where(dt => dt.HasValue && dt.Value != DateTime.MinValue)
            .ToList();

        return activityTimes.Any() ? activityTimes.Max() : null;
    }

    /// <summary>
    /// called by the AdminManager periodically to update courier activity statuses.
    /// </summary>
    /// <param name="oldClock"></param>
    /// <param name="newClock"></param>
    internal static void PeriodicCourierUpdates(DateTime oldClock, DateTime newClock)
    {
        // check all couriers for inactivity
        var allCouriers = s_dal.Courier.ReadAll().ToList();
        TimeSpan inactivityTimeSpan = s_dal.Config.InactivityTimeRange;
        if (inactivityTimeSpan == TimeSpan.Zero) return;

        //check each courier for inactivity and update status if needed
        foreach (var doCourier in allCouriers)
        {
            // only check active couriers
            if (doCourier.IsActive)
            {
                DateTime? lastActivityTime = GetLastActivityTime(doCourier.Id);

                if (lastActivityTime.HasValue && newClock - lastActivityTime.Value > inactivityTimeSpan)
                {
                    s_dal.Courier.Update(doCourier with { IsActive = false });

                    // notify observers about the update
                    Observers.NotifyItemUpdated(doCourier.Id);
                }
            }
        }
    }

    /// <summary>
    /// calculates the number of on-time and late deliveries for a courier.
    /// </summary>
    /// <param name="courierId"></param>
    /// <returns>
    /// deliveredOnTime: number of deliveries completed on or before the maximum delivery time.
    /// </returns>
    internal static (int DeliveredOnTime, int DeliveredLate) GetCourierStatistics(int courierId)
    {
        // retrieve all completed deliveries for the courier
        var completedDeliveries = s_dal.Delivery.ReadAll(d =>
            d.CourierId == courierId &&
            d.OrderClosedStatus == DO.OrderEndStatus.Delivered &&
            d.DeliveryEndTime.HasValue)
            .ToList();

        // calculate on-time vs late deliveries
        int deliveredOnTime = 0;
        int deliveredLate = 0;

        // assume using System.Linq; is present
        foreach (var delivery in completedDeliveries)
        {
            DateTime maxDeliveryTime = OrderManager.CalculateMaxDeliveryTime(delivery.OrderId);

            // compare delivery end time to max delivery time
            if (delivery.DeliveryEndTime!.Value <= maxDeliveryTime)
                deliveredOnTime++;
            else
                deliveredLate++;
        }

        return (deliveredOnTime, deliveredLate);
    }

    /// <summary>
    /// finds an open delivery (in-progress) for a given courier.
    /// </summary>
    /// <param name="courierId"></param>
    /// <returns></returns>
    internal static DO.Delivery? FindOpenDeliveryForCourier(int courierId)
    {
        return s_dal.Delivery.ReadAll(d => d.CourierId == courierId && d.DeliveryEndTime == null).FirstOrDefault();
    }

    /// <summary>
    /// maps an open DO.Delivery to a BO.OrderInProgress entity.
    /// </summary>
    /// <param name="openDelivery"></param>
    /// <returns>
    /// orderInProgress: the mapped BO.OrderInProgress object.
    /// </returns>
    internal static BO.OrderInProgress MapOpenDeliveryToOrderInProgress(DO.Delivery openDelivery)
    {
        // read base entities
        DO.Order doOrder = s_dal.Order.Read(openDelivery.OrderId)!;
        DO.Courier doCourier = s_dal.Courier.Read(openDelivery.CourierId)!;

        // calculate statuses and times
        DateTime maxDeliveryTime = OrderManager.CalculateMaxDeliveryTime(doOrder.Id);
        BO.ScheduleStatus scheduleStatus = OrderManager.CalculateScheduleStatus(doOrder.Id);

        // calculate logistics info
        double airDistance = Tools.GetAirDistance(
            s_dal.Config.CompenyLatitude ?? 0, s_dal.Config.CompenyLongitude ?? 0,
            doOrder.Latitude, doOrder.Longitude);

        // get actual distance and estimated time
        var travelInfo = Tools.GetActualDistanceAndEstimatedTimeSync(
            s_dal.Config.CompenyLatitude ?? 0, s_dal.Config.CompenyLongitude ?? 0,
            doOrder.Latitude, doOrder.Longitude, (BO.DeliveryType)doCourier.TypeOfDelivery); // cast DO to BO

        // calculate ExpectedDeliveryTime
        // assume DeliveryStartTime is Non-Nullable
        DateTime expectedDeliveryTime = travelInfo.HasValue ? openDelivery.DeliveryStartTime.Add(travelInfo.Value.EstimatedTime) : default;

        // build BO.OrderInProgress
        return new BO.OrderInProgress
        {
            DeliveryId = openDelivery.Id,
            OrderId = doOrder.Id,
            TypeOfOrder = (BO.OrderType)doOrder.TypeOfOrder,
            Description = doOrder.Description,
            Address = doOrder.Address,
            AirDistance = airDistance,
            ActualDistance = travelInfo?.ActualDistance ?? 0,
            CustomerName = doOrder.CustomerName,
            CustomerPhone = doOrder.CustomerPhone,
            OrderOpeningTime = doOrder.OrderOpeningTime,
            DeliveryStartTime = openDelivery.DeliveryStartTime,
            ExpectedDeliveryTime = expectedDeliveryTime,
            MaxDeliveryTime = maxDeliveryTime,
            StatusOfOrder = BO.OrderStatus.InProgress,
            TimeLinessStatus = scheduleStatus,
            RemainingDeliveryTime = maxDeliveryTime - AdminManager.Now
        };
    }

    /// <summary>
    /// authenticates a user and returns their role.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    internal static BO.UserRole Login(int userId, string password)
    {
        // first, check if the user is admin
        if (userId == s_dal.Config.AdminId)
        {
            if (Tools.VerifyPassword(password, s_dal.Config.AdminPassword))
                return BO.UserRole.Admin;
        }

        // then, check if the user is a courier
        try
        {
            // trying to read the courier record
            DO.Courier doCourier = s_dal.Courier.Read(userId)!;

            // verify the password
            if (Tools.VerifyPassword(password, doCourier.Password))
                return BO.UserRole.Courier;
        }
        catch (BO.BlDoesNotExistException) // if courier not found
        {
            // continue to next step to throw a general failure exception.
        }

        // if neither admin nor courier login succeeded
        throw new BO.BlLoginFailedException("Login failed: Invalid ID or password.");
    }

    /// <summary>
    /// gets an existing courier or throws if not found.
    /// </summary>
    /// <param name="courierId"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    internal static DO.Courier GetExistingCourier(int courierId)
    {
        // retrieve the courier from DAL
        var courier = s_dal.Courier.Read(courierId);

        // throw if not found
        if (courier == null)
            throw new BO.BlDoesNotExistException($"Courier with ID {courierId} does not exist in the system.");
        return courier;
    }

    /// <summary>
    /// gets whether a courier is associated with any deliveries.
    /// </summary>
    /// <param name="courierId"></param>
    /// <returns></returns>
    internal static bool IsCourierUsed(int courierId)
    {
        // check for any deliveries linked to the courier
        return s_dal.Delivery.ReadAll(d => d.CourierId == courierId).Any();
    }

    /// <summary>
    /// sorts a collection of couriers based on the specified field.
    /// </summary>
    /// <param name="couriers"></param>
    /// <param name="sortBy"></param>
    /// <returns></returns>
    public static IEnumerable<BO.CourierInList> SortCouriersBy(IEnumerable<BO.CourierInList> couriers, BO.CourierFieldSort sortBy)
    {
        // sorting logic based on the specified field
        return sortBy switch
        {
            BO.CourierFieldSort.Id => couriers.OrderBy(c => c.Id),
            BO.CourierFieldSort.Name => couriers.OrderBy(c => c.Name),
            BO.CourierFieldSort.IsActive => couriers.OrderBy(c => c.IsActive),
            BO.CourierFieldSort.TypeOfDelivery => couriers.OrderBy(c => c.TypeOfDelivery),
            BO.CourierFieldSort.StartWorkTime => couriers.OrderBy(c => c.StartWorkTime),
            BO.CourierFieldSort.TotalOnTimeDeliveries => couriers.OrderByDescending(c => c.TotalOnTimeDeliveries),
            BO.CourierFieldSort.TotalLateDeliveries => couriers.OrderBy(c => c.TotalLateDeliveries),

            //default case
            _ => couriers.OrderBy(c => c.Id)
        };
    }

    /// <summary>
    /// Validates the data of a BO.Courier object.
    /// </summary>
    /// <param name="boCourier"></param>
    /// <exception cref="BO.BlInvalidDataException"></exception>
    internal static void ValidateCourierData(BO.Courier boCourier)
    {
        // Validate ID is a 9-digit number
        if (boCourier.Id < 100000000 || boCourier.Id > 999999999)
            throw new BO.BlInvalidDataException($"ID must be a 9-digit number");

        // Validate Name
        if (string.IsNullOrWhiteSpace(boCourier.Name) || boCourier.Name.Length < 2)
            throw new BO.BlInvalidDataException($"Courier name must contain at least 2 characters.");

        // Validate Phone (10 digits, starts with 0)
        string phoneToValidate = boCourier.Phone?.Replace("-", "").Replace(" ", "") ?? "";
        const string phonePattern = @"^0\d{9}$";

        if (string.IsNullOrEmpty(phoneToValidate) || !Regex.IsMatch(phoneToValidate, phonePattern))
            throw new BO.BlInvalidDataException($"Phone number '{boCourier.Phone}' is invalid. Must be 10 digits starting with 0.");

        // Validate Email format
        const string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

        if (string.IsNullOrEmpty(boCourier.Email) || !Regex.IsMatch(boCourier.Email, emailPattern))
            throw new BO.BlInvalidDataException($"Email address '{boCourier.Email}' is not in a valid format.");

        // Validate Password (at least 8 characters)
        if (string.IsNullOrEmpty(boCourier.Password) || boCourier.Password.Length < 8)
            throw new BO.BlInvalidDataException($"Password must contain at least 8 characters.");

        // Validate MaxDistance
        if (boCourier.MaxDistance.HasValue)
        {
            if (boCourier.MaxDistance.Value <= 0)
                throw new BO.BlInvalidDataException($"Maximum distance must be a positive number.");

            // Retrieve current configuration
            BO.Config currentConfig = AdminManager.GetConfig();

            //if the company has a limit for max delivery distance, ensure courier's max distance does not exceed it
            if (currentConfig.DeliveryMaxDistance.HasValue)
            {
                if (boCourier.MaxDistance.Value > currentConfig.DeliveryMaxDistance.Value)
                {
                    throw new BO.BlInvalidDataException(
                        $"Courier's max distance ({boCourier.MaxDistance.Value} km) cannot exceed the company's max distance ({currentConfig.DeliveryMaxDistance.Value} km).");
                }
            }
        }

        // Validate TypeOfDelivery
        if (!Enum.IsDefined(typeof(BO.DeliveryType), boCourier.TypeOfDelivery))
            throw new BO.BlInvalidDataException($"The provided Delivery Type ({boCourier.TypeOfDelivery}) is not a valid option.");
    }
}
