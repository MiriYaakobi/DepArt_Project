using DalApi;

namespace Helpers;

/// <summary>
/// Manages courier-related operations.
/// </summary>
internal static class CourierManager
{
    private static IDal s_dal = Factory.Get;

    /// <summary>
    /// Creates a new courier in the system.
    /// </summary>
    internal static void CreateCourier(BO.Courier courier)
    {
        // Validate input by calling the helper method
        AssertCourierInputValidity(courier);

        // Check for duplicate ID (InvalidOperationException)
        try
        {
            s_dal.Courier.Read(courier.Id);
            throw new InvalidOperationException($"Courier with ID {courier.Id} already exists.");
        }
        catch (DO.DalDoesNotExistException)
        {
            // השליח לא קיים - תקין להמשיך
        }

        // Mapping BO to DO and creating the courier
        DO.Courier doCourier = new DO.Courier
        (
            Id: courier.Id,
            Name: courier.Name!,
            Phone: courier.Phone!,
            Email: courier.Email!,
            Password: courier.Password!,
            IsActive: true, // courier is active upon creation
            TypeOfDelivery: (DO.DeliveryType)courier.TypeOfDelivery,
            StartWorkTime: AdminManager.Now, // Start work time is the system clock
            MaxDistance: courier.MaxDistance
        );
        s_dal.Courier.Create(doCourier);
    }

    /// <summary>
    /// Reads all couriers from the DAL and converts them to BO.CourierInList entities.
    /// </summary>
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
        // TO_DO: המיון הלוגי (sort) ימומש בממשק ICourier.ReadAll, לאחר קבלת ה-IEnumerable.
    }

    /// <summary>
    /// Reads a courier record from the DAL, maps it to BO, and attaches logical and statistical data.
    /// </summary>
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
        {
            currentOrder = MapOpenDeliveryToOrderInProgress(openDelivery);
        }

        // Mapping to BO
        return new BO.Courier
        {
            Id = doCourier.Id,
            Name = doCourier.Name,
            Phone = doCourier.Phone,
            Email = doCourier.Email,
            Password = doCourier.Password,
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
    /// Updates an existing courier's information in the system.
    /// </summary>
    internal static void UpdateCourier(BO.Courier courier)
    {
        // retrieve existing courier or throw if not found by calling helper method
        DO.Courier existingCourier = GetExistingCourier(courierId: courier.Id);

        // validate input by calling the helper method
        AssertCourierInputValidity(courier);

        // map updated fields
        DO.Courier updatedCourier = existingCourier with
        {
            Name = courier.Name!,
            Phone = courier.Phone!,
            Email = courier.Email!,
            Password = courier.Password!,
            TypeOfDelivery = (DO.DeliveryType)courier.TypeOfDelivery,
            MaxDistance = courier.MaxDistance
        };

        // perform the update
        s_dal.Courier.Update(updatedCourier);
    }

    /// <summary>
    /// Deletes a courier from the system after checking for dependencies.
    /// </summary>
    internal static void DeleteCourier(int courierId)
    {
        // check for existing deliveries
        if (s_dal.Delivery.ReadAll(d => d.CourierId == courierId).Any())
        {
            throw new InvalidOperationException($"Cannot delete Courier {courierId}: courier is associated with existing deliveries (history).");
        }

        // perform deletion
        try
        {
            s_dal.Courier.Delete(courierId);
        }
        catch (DO.DalDoesNotExistException)
        {
            throw new InvalidOperationException($"Courier with ID {courierId} does not exist and cannot be deleted.");
        }
    }

    /// <summary>
    /// helper method to get the last activity time of a courier based on their deliveries.
    /// </summary>
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
    /// called by AdminManager.UpdateClock to perform periodic updates on couriers based on inactivity.
    /// </summary>
    internal static void PeriodicCourierUpdates(DateTime oldClock, DateTime newClock)
    {
        // check all couriers for inactivity
        var allCouriers = s_dal.Courier.ReadAll().ToList();
        TimeSpan inactivityTimeSpan = s_dal.Config.InactivityTimeRange;
        if (inactivityTimeSpan == TimeSpan.Zero) return;

        foreach (var doCourier in allCouriers)
        {
            if (doCourier.IsActive)
            {
                DateTime? lastActivityTime = GetLastActivityTime(doCourier.Id);

                if (lastActivityTime.HasValue && newClock - lastActivityTime.Value > inactivityTimeSpan)
                {
                    s_dal.Courier.Update(doCourier with { IsActive = false });
                }
            }
        }
    }

    /// <summary>
    /// Calculates delivery statistics for a given courier.
    /// </summary>
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
    /// helper method to find an open delivery for a given courier.
    /// </summary>
    internal static DO.Delivery? FindOpenDeliveryForCourier(int courierId)
    {
        return s_dal.Delivery.ReadAll(d => d.CourierId == courierId && d.DeliveryEndTime == null).FirstOrDefault();
    }

    /// <summary>
    /// Maps an open delivery to an in-progress order.
    /// </summary>
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
    /// Authenticates a user (admin or courier) based on provided credentials.
    /// </summary>
    internal static BO.UserRole Login(int userId, string password)
    {
        // first, check if the user is admin
        if (userId == s_dal.Config.AdminId)
        {
            if (password == s_dal.Config.AdminPassword)
            {
                return BO.UserRole.Admin;
            }
        }

        // then, check if the user is a courier
        try
        {
            // trying to read the courier record
            DO.Courier doCourier = s_dal.Courier.Read(userId)!;

            if (doCourier.Password == password)
            {
                // successful courier login
                return BO.UserRole.Courier;
            }
        }
        catch (DO.DalDoesNotExistException) // if courier not found
        {
            // continue to next step to throw a general failure exception.
        }

        // if neither admin nor courier login succeeded
        throw new ArgumentException("Login failed: Invalid ID or password.");
    }

    /// <summary>
    /// helper method to get an existing courier or throw if not found.
    /// </summary>
    private static DO.Courier GetExistingCourier(int courierId)
    {
        try
        {
            // retrieve existing courier
            return s_dal.Courier.Read(courierId)!;
        }
        catch (DO.DalDoesNotExistException)
        {
            // courier not found
            throw new InvalidOperationException($"Courier with ID {courierId} does not exist.");
        }
    }

    /// <summary>
    /// helper method to validate courier input data.
    /// </summary>
    private static void AssertCourierInputValidity(BO.Courier courier)
    {
        if (string.IsNullOrEmpty(courier.Name) || courier.Name.Length < 2)
            throw new ArgumentException("Courier name must contain at least 2 characters.");
        if (string.IsNullOrEmpty(courier.Phone) || courier.Phone.Length != 10 || !courier.Phone.All(char.IsDigit))
            throw new ArgumentException("Phone number is invalid.");
        if (string.IsNullOrEmpty(courier.Password) || courier.Password.Length < 6)
            throw new ArgumentException("Password must be at least 6 characters long.");
        if (courier.MaxDistance.HasValue && courier.MaxDistance.Value <= 0)
            throw new ArgumentException("Max distance must be a positive value.");
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
    public static IEnumerable<BO.CourierInList> SortCouriersBy(IEnumerable<BO.CourierInList> couriers,
                                                                            BO.CourierFieldSort sortBy)
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
}