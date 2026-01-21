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

    // Stage 7: Mutex to prevent overlapping periodic updates
    private static readonly AsyncMutex s_periodicMutex = new();

    // Stage 7: Mutex to prevent overlapping simulation executions
    private static readonly AsyncMutex s_simulationMutex = new();

    // Stage 7: Random generator for simulation logic
    private static readonly Random s_rand = new();

    /// <summary>
    /// creates a new courier after validating input and checking for duplicates.
    /// </summary>
    /// <param name="courier"></param>
    /// <exception cref="BO.BlAlreadyExistsException"></exception>
    internal static void CreateCourier(BO.Courier courier)
    {
        AdminManager.ThrowOnSimulatorIsRunning();

        // Validate input by calling the helper method
        ValidatePassword(courier.Password!);
        ValidateCourierData(courier);

        string hashedPassword = Tools.HashPassword(courier.Password!);

        lock (AdminManager.BlMutex)
        {
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
        }

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
        IEnumerable<DO.Courier> dalCouriers;
        lock (AdminManager.BlMutex)
        {
            // filter by isActive if provided
            dalCouriers = s_dal.Courier.ReadAll(isActive.HasValue ? d => d.IsActive == isActive.Value : null);
        }

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
    /// <exception cref="Exception"></exception>
    internal static void UpdateCourier(BO.Courier courier)
    {
        AdminManager.ThrowOnSimulatorIsRunning();

        // Retrieve existing courier or throw if not found
        DO.Courier existingCourier = GetExistingCourier(courierId: courier.Id);

        // Prevent deactivation if courier has an active order
        if (courier.IsActive == false && courier.CurrentOrder != null)
        {
            throw new Exception("Cannot deactivate courier while they have an active order assigned.");
        }

        // Determine password to store
        string passwordToStore = (string.IsNullOrEmpty(courier.Password) || courier.Password == "********")
            ? existingCourier.Password
            : Tools.HashPassword(courier.Password);

        // Map updated fields
        DO.Courier updatedCourier = existingCourier with
        {
            Name = courier.Name!,
            Phone = courier.Phone!,
            Email = courier.Email!,
            Password = passwordToStore,
            IsActive = courier.IsActive,
            TypeOfDelivery = (DO.DeliveryType)courier.TypeOfDelivery,
            MaxDistance = courier.MaxDistance
        };

        lock (AdminManager.BlMutex)
        {
            s_dal.Courier.Update(updatedCourier);
        }

        // Notify observers about the update
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
        AdminManager.ThrowOnSimulatorIsRunning();

        lock (AdminManager.BlMutex)
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
        lock (AdminManager.BlMutex)
        {
            // assume using System.Linq; is present
            var courierDeliveries = s_dal.Delivery.ReadAll(d => d.CourierId == courierId);
            var activityTimes = courierDeliveries
                .SelectMany(d => new[] { d.DeliveryStartTime, d.DeliveryEndTime })
                .Where(dt => dt.HasValue && dt.Value != DateTime.MinValue)
                .ToList();

            return activityTimes.Any() ? activityTimes.Max() : null;
        }
    }

    /// <summary>
    /// called by the AdminManager periodically to update courier activity statuses (deactivate if inactive).
    /// </summary>
    /// <param name="oldClock"></param>
    /// <param name="newClock"></param>
    internal static void PeriodicCourierUpdates(DateTime oldClock, DateTime newClock)
    {
        // Check if the update is already in progress. If so, skip this cycle.
        if (s_periodicMutex.CheckAndSetInProgress())
            return;

        try
        {
            List<DO.Courier> allCouriers;
            lock (AdminManager.BlMutex)
            {
                allCouriers = s_dal.Courier.ReadAll().ToList();
            }

            // List to collect IDs for notification (to notify outside of locks)
            List<int> couriersToNotify = new();

            foreach (var doCourier in allCouriers)
            {
                // only check active couriers
                if (doCourier.IsActive)
                {
                    bool isShipping = FindOpenDeliveryForCourier(doCourier.Id) != null; // Has internal lock

                    if (!isShipping)
                    {
                        DateTime? lastActivityTime = GetLastActivityTime(doCourier.Id); // Has internal lock
                        TimeSpan inactivityTimeSpan;
                        lock (AdminManager.BlMutex) { inactivityTimeSpan = s_dal.Config.InactivityTimeRange; }

                        if (inactivityTimeSpan != TimeSpan.Zero && lastActivityTime.HasValue)
                        {
                            if (newClock - lastActivityTime.Value > inactivityTimeSpan)
                            {
                                lock (AdminManager.BlMutex)
                                {
                                    s_dal.Courier.Update(doCourier with { IsActive = false });
                                }
                                couriersToNotify.Add(doCourier.Id);
                            }
                        }
                    }
                }
            }

            // Notify observers outside of any lock
            foreach (int id in couriersToNotify)
            {
                Observers.NotifyItemUpdated(id);
            }
        }
        finally
        {
            // Always release the mutex at the end
            s_periodicMutex.UnsetInProgress();
        }
    }

    /// <summary>
    /// SIMULATION: Simulates courier activity (picking orders, delivering).
    /// </summary>
    internal static async Task SimulateCourierActivityAsync()
    {
        // Check mutual exclusion
        if (s_simulationMutex.CheckAndSetInProgress())
            return;

        try
        {
            List<DO.Courier> activeCouriers;

            // Fetch active couriers under lock
            lock (AdminManager.BlMutex)
            {
                activeCouriers = s_dal.Courier.ReadAll(c => c.IsActive).ToList();
            }

            // Iterate over each active courier
            foreach (var courier in activeCouriers)
            {
                // Is this courier already busy?
                // We'll use the existing helper function in this file
                DO.Delivery? openDelivery = FindOpenDeliveryForCourier(courier.Id);

                if (openDelivery != null)
                {
                    // The courier is busy - they cannot take a new order.
                    continue;
                }

                // The courier is available! Let's check if they have the energy to look for work (small probability to avoid overload)
                // Assume a 30% chance each tick that they will open the app
                if (s_rand.NextDouble() > 0.3) continue;

                try
                {
                    // Search for open orders that match the courier (by distance and type)
                    // This is a potentially heavy operation, so we run it in a separate task
                    var potentialOrders = await Task.Run(() =>
                        DeliveryManager.GetAvailableOpenOrders(courier.Id).ToList());

                    if (potentialOrders.Any())
                    {
                        // The courier found some potential orders!
                        var selectedOrder = potentialOrders[s_rand.Next(potentialOrders.Count)];

                        // Retrieve the full order data to get accurate coordinates
                        DO.Order fullOrderData = OrderManager.GetExistingOrder(selectedOrder.OrderId);

                        // Create the delivery (the courier takes the order)
                        await DeliveryManager.CreateNewDeliveryForOrderAsync(
                            selectedOrder.OrderId,
                            courier.Id,
                            fullOrderData.Latitude,
                            fullOrderData.Longitude,
                            (BO.DeliveryType)courier.TypeOfDelivery
                        );
                    }
                }
                catch
                {
                    // Ignore any errors during simulation to keep it running smoothly
                    continue;
                }
            }
        }
        finally
        {
            s_simulationMutex.UnsetInProgress();
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
        List<DO.Delivery> completedDeliveries;
        lock (AdminManager.BlMutex)
        {
            // retrieve all completed deliveries for the courier
            completedDeliveries = s_dal.Delivery.ReadAll(d =>
                d.CourierId == courierId &&
                d.OrderClosedStatus == DO.OrderEndStatus.Delivered &&
                d.DeliveryEndTime.HasValue)
                .ToList();
        }

        // calculate on-time vs late deliveries
        int deliveredOnTime = 0;
        int deliveredLate = 0;

        // assume using System.Linq; is present
        foreach (var delivery in completedDeliveries)
        {
            DateTime maxDeliveryTime = OrderManager.CalculateMaxDeliveryTime(delivery.OrderId); // Has lock inside

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
        lock (AdminManager.BlMutex)
        {
            return s_dal.Delivery.ReadAll(d => d.CourierId == courierId && d.DeliveryEndTime == null).FirstOrDefault();
        }
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
        DO.Order doOrder;
        DO.Courier doCourier;
        double companyLat, companyLon;

        lock (AdminManager.BlMutex)
        {
            // retrieve associated order and courier
            doOrder = s_dal.Order.Read(openDelivery.OrderId)!;
            doCourier = s_dal.Courier.Read(openDelivery.CourierId)!;
            companyLat = s_dal.Config.CompenyLatitude ?? 0;
            companyLon = s_dal.Config.CompenyLongitude ?? 0;
        }

        // calculate max delivery time and schedule status
        DateTime maxDeliveryTime = OrderManager.CalculateMaxDeliveryTime(doOrder.Id); // Has lock inside
        BO.ScheduleStatus scheduleStatus = OrderManager.CalculateScheduleStatus(doOrder.Id); // Has lock inside

        // calculate air distance
        double airDistance = Tools.GetAirDistance(
                companyLat, companyLon,
                doOrder.Latitude, doOrder.Longitude);

        // calculate saved actual distance
        double savedDistance = openDelivery.ActualDistance ?? 0;

        // if no saved distance, use air distance as fallback
        if (savedDistance == 0)
            savedDistance = airDistance;

        // calculate estimated delivery time based on courier speed
        double speed = doCourier.TypeOfDelivery switch
        {
            DO.DeliveryType.ByFoot => 5.0,
            DO.DeliveryType.Bicycle => 20.0,
            DO.DeliveryType.Motorcycle => 60.0,
            _ => 50.0
        };

        TimeSpan estimatedTime = TimeSpan.FromHours(savedDistance / speed);

        // map to BO.OrderInProgress
        return new BO.OrderInProgress
        {
            DeliveryId = openDelivery.Id,
            OrderId = doOrder.Id,
            TypeOfOrder = (BO.OrderType)doOrder.TypeOfOrder,
            Description = doOrder.Description,
            Address = doOrder.Address,
            CustomerName = doOrder.CustomerName,
            CustomerPhone = doOrder.CustomerPhone,
            OrderOpeningTime = doOrder.OrderOpeningTime,
            DeliveryStartTime = openDelivery.DeliveryStartTime,
            AirDistance = airDistance,
            ActualDistance = savedDistance,
            ExpectedDeliveryTime = openDelivery.DeliveryStartTime.Add(estimatedTime),
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
        lock (AdminManager.BlMutex)
        {
            // check for valid input
            if (s_dal.Config != null && userId == s_dal.Config.AdminId)
            {
                string storedPass = s_dal.Config.AdminPassword ?? "";

                // verify password (both hashed and plain text for backward compatibility)
                bool isMatch = (Tools.VerifyPassword(password, storedPass)) || (password == storedPass);

                if (isMatch)
                    return BO.UserRole.Admin;
            }

            // check for courier
            DO.Courier? doCourier = null;
            try { doCourier = s_dal.Courier.Read(userId); } catch { }

            if (doCourier != null)
            {
                if (!doCourier.IsActive)
                    throw new BO.BlLoginFailedException("Account is inactive.");

                string storedPass = doCourier.Password ?? "";

                // verify password (both hashed and plain text for backward compatibility)
                bool isMatch = (Tools.VerifyPassword(password, storedPass)) || (password == storedPass);

                if (isMatch)
                    return BO.UserRole.Courier;

                // incorrect password
                throw new BO.BlLoginFailedException("Incorrect password.");
            }
        }

        // user ID not found
        throw new BO.BlLoginFailedException("User ID not found or incorrect password.");
    }

    /// <summary>
    /// gets an existing courier or throws if not found.
    /// </summary>
    /// <param name="courierId"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    internal static DO.Courier GetExistingCourier(int courierId)
    {
        lock (AdminManager.BlMutex)
        {
            // retrieve the courier from DAL
            var courier = s_dal.Courier.Read(courierId);

            // throw if not found
            if (courier == null)
                throw new BO.BlDoesNotExistException($"Courier with ID {courierId} does not exist in the system.");
            return courier;
        }
    }

    /// <summary>
    /// gets whether a courier is associated with any deliveries.
    /// </summary>
    /// <param name="courierId"></param>
    /// <returns></returns>
    internal static bool IsCourierUsed(int courierId)
    {
        lock (AdminManager.BlMutex)
        {
            // check for any deliveries linked to the courier
            return s_dal.Delivery.ReadAll(d => d.CourierId == courierId).Any();
        }
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

        if (!string.IsNullOrEmpty(boCourier.Password) && boCourier.Password.Length < 8)
            throw new BO.BlInvalidDataException("Password must contain at least 8 characters.");

        // Validate MaxDistance
        if (boCourier.MaxDistance.HasValue)
        {
            if (boCourier.MaxDistance.Value <= 0)
                throw new BO.BlInvalidDataException($"Maximum distance must be a positive number.");

            lock (AdminManager.BlMutex)
            {
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
        }

        // Validate TypeOfDelivery
        if (!Enum.IsDefined(typeof(BO.DeliveryType), boCourier.TypeOfDelivery))
            throw new BO.BlInvalidDataException($"The provided Delivery Type ({boCourier.TypeOfDelivery}) is not a valid option.");
    }

    /// <summary>
    /// validates the password according to defined rules.
    /// </summary>
    /// <param name="password"></param>
    /// <exception cref="BO.BlInvalidDataException"></exception>
    internal static void ValidatePassword(string password)
    {
        if (string.IsNullOrEmpty(password) || password.Length < 8)
            throw new BO.BlInvalidDataException("Password must contain at least 8 characters.");
    }
}