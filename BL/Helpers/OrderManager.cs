using DalApi;

namespace Helpers;

/// <summary>
/// Manages order-related operations.
/// </summary>
internal static class OrderManager
{
    private static IDal s_dal = Factory.Get;

    /// <summary>
    /// Creates a new order after validating input and geocoding the address.
    /// </summary>
    /// <param name="order">BO.Order entity to create.</param>
    /// <exception cref="ArgumentException">If the order data is invalid (including address not found).</exception>
    internal static void CreateOrder(BO.Order order)
    {
        // verify input validity
        AssertOrderInputValidity(order);

        // Geocoding the address to get coordinates
        var coordinates = Tools.GetCoordinatesOfAddressSync(order.Address!);
        if (coordinates == null)
        {
            // If the customer's address is invalid, throw an exception
            throw new BO.BlInvalidDataException($"Address '{order.Address}' is invalid or could not be found.");
        }

        // Mapping and saving to DAL
        DO.Order doOrder = new DO.Order
        (
            Id: default, // ID will be assigned by DAL
            TypeOfOrder: (DO.OrderType)order.TypeOfOrder,
            Address: order.Address!,
            Latitude: coordinates.Value.Latitude, // Using geocoded latitude
            Longitude: coordinates.Value.Longitude,
            CustomerName: order.CustomerName!,
            CustomerPhone: order.CustomerPhone!,
            OrderOpeningTime: AdminManager.Now,
            PackageDetails: order.PackageDetails,
            Description: order.Description
        );

        s_dal.Order.Create(doOrder);
    }

    /// <summary>
    /// Reads a single order by ID, calculating its statuses and related data.
    /// </summary>
    internal static BO.Order ReadOrder(int orderId)
    {
        // retrieve existing order
        DO.Order doOrder = GetExistingOrder(orderId);

        // calculate statuses and related data
        BO.OrderStatus statusOfOrder = CalculateOrderStatus(orderId);
        BO.ScheduleStatus timeLinessStatus = CalculateScheduleStatus(orderId);
        DateTime maxDeliveryTime = CalculateMaxDeliveryTime(orderId);
        TimeSpan remainingDeliveryTime = maxDeliveryTime - AdminManager.Now;

        // calculate air distance from company to order location
        double airDistance = Tools.GetAirDistance(
            s_dal.Config.CompenyLatitude ?? 0,
            s_dal.Config.CompenyLongitude ?? 0,
            doOrder.Latitude,
            doOrder.Longitude
        );

        // retrieve delivery list for the order
        IEnumerable<BO.DeliveryPerOrderInList> deliveryListCollection = MapDeliveryListForOrder(orderId);
        BO.DeliveryPerOrderInList? deliveryList = deliveryListCollection.FirstOrDefault();

        // calculate expected delivery time if order is InProgress
        DateTime? expectedDeliveryTime = null;

        // calculation is only required when the order is InProgress
        if (statusOfOrder == BO.OrderStatus.InProgress)
        {
            // find the only open delivery (DeliveryEndTime == null)
            DO.Delivery? openDelivery = s_dal.Delivery.ReadAll(d => d.OrderId == orderId && d.DeliveryEndTime == null).FirstOrDefault();

            if (openDelivery != null)
            {
                // calculate estimated delivery duration by calling helper method
                // EstimatedDuration = AirDistance / ShipperSpeed
                TimeSpan estimatedDuration = CalculateEstimatedDeliveryDuration(openDelivery, doOrder);
                // ExpectedDeliveryTime = DeliveryStartTime + estimatedDuration
                expectedDeliveryTime = openDelivery.DeliveryStartTime.Add(estimatedDuration);
            }
        }

        // return the mapped BO.Order
        return new BO.Order
        {
            Id = doOrder.Id,
            TypeOfOrder = (BO.OrderType)doOrder.TypeOfOrder,
            Description = doOrder.Description,
            Address = doOrder.Address,

            Latitude = doOrder.Latitude,
            Longitude = doOrder.Longitude,
            AirDistance = airDistance,

            CustomerName = doOrder.CustomerName,
            CustomerPhone = doOrder.CustomerPhone,
            PackageDetails = doOrder.PackageDetails,
            OrderOpeningTime = doOrder.OrderOpeningTime,

            ExpectedDeliveryTime = expectedDeliveryTime,

            MaxDeliveryTime = maxDeliveryTime,
            StatusOfOrder = statusOfOrder,
            TimeLinessStatus = timeLinessStatus,

            RemainingDeliveryTime = remainingDeliveryTime,

            DeliveryList = deliveryList
        };
    }

    /// <summary>
    /// Reads all orders, calculating their statuses and related data for each.
    /// </summary>
    internal static IEnumerable<BO.OrderInList> ReadAllOrders()
    {
        var dalOrders = s_dal.Order.ReadAll().ToList();
        var companyCoords = GetCompanyCoordinates();

        return dalOrders.Select(doOrder =>
        {
            int orderId = doOrder.Id;

            // find active delivery if any
            var activeDelivery = s_dal.Delivery.ReadAll(d => d.OrderId == orderId && d.DeliveryEndTime == null).FirstOrDefault();

            // calculate statuses
            BO.OrderStatus statusOfOrder = CalculateOrderStatus(orderId);
            BO.ScheduleStatus timeLinessStatus = CalculateScheduleStatus(orderId);

            // calculate total deliveries and handling duration
            int totalDeliveries = s_dal.Delivery.ReadAll(d => d.OrderId == orderId).Count();
            TimeSpan totalHandlingDuration = TimeSpan.Zero; // default value

            // find last closed delivery
            DO.Delivery? lastClosedDelivery = s_dal.Delivery.ReadAll(d => d.OrderId == orderId)
                                                            .Where(d => d.DeliveryEndTime.HasValue)
                                                            .OrderByDescending(d => d.DeliveryEndTime)
                                                            .FirstOrDefault();

            // If a closed delivery is found
            if (lastClosedDelivery != null)
            {
                // if the order is in a final state, calculate total handling duration
                if (CalculateOrderStatus(orderId) == BO.OrderStatus.Delivered ||
                    CalculateOrderStatus(orderId) == BO.OrderStatus.Refused ||
                    CalculateOrderStatus(orderId) == BO.OrderStatus.Cancelled)
                {
                    // calculate the difference between the last delivery end time and the order opening time
                    totalHandlingDuration = lastClosedDelivery.DeliveryEndTime!.Value - doOrder.OrderOpeningTime;
                }
                // Otherwise (the order is still open, like "customer not found" or "failed"), leave TotalHandlingDuration = TimeSpan.Zero
            }

            // calculate remaining time and air distance
            DateTime maxDeliveryTime = CalculateMaxDeliveryTime(orderId);
            TimeSpan remainingTime = maxDeliveryTime - AdminManager.Now;
            double airDistance = Tools.GetAirDistance(companyCoords.Latitude, companyCoords.Longitude, doOrder.Latitude, doOrder.Longitude);

            return new BO.OrderInList
            {
                Id = activeDelivery?.Id,
                OrderId = orderId,
                TypeOfOrder = (BO.OrderType)doOrder.TypeOfOrder,
                AirDistance = airDistance,
                StatusOfOrder = statusOfOrder,
                TimeLinessStatus = timeLinessStatus,
                RemainingTime = remainingTime,
                TotalHandlingDuration = totalHandlingDuration,
                TotalDeliveries = totalDeliveries,
                MaxDeliveryTime = maxDeliveryTime
            };
        });
    }

    /// <summary>
    /// Updates an existing order after validating its status and input data.
    /// </summary>
    /// <param name="order"></param>
    /// <exception cref="InvalidOperationException"></exception>
    internal static void UpdateOrder(BO.Order order)
    {
        // check for existing order and validate input
        DO.Order existingOrder = GetExistingOrder(order.Id);
        AssertOrderInputValidity(order);

        // check order status
        BO.OrderStatus currentStatus = CalculateOrderStatus(order.Id);
        if (currentStatus == BO.OrderStatus.Delivered || currentStatus == BO.OrderStatus.Refused || currentStatus == BO.OrderStatus.Cancelled)
        {
            throw new BO.BlInvalidOperationException($"Cannot update Order {order.Id}. Status is {currentStatus}.");
        }

        // update fields
        DO.Order updatedOrder = existingOrder with
        {
            TypeOfOrder = (DO.OrderType)order.TypeOfOrder,
            Description = order.Description,
            PackageDetails = order.PackageDetails,
            CustomerName = order.CustomerName!,
            CustomerPhone = order.CustomerPhone!
            // Address, Latitude, Longitude, OrderOpeningTime cannot be changed
        };

        // save updates
        s_dal.Order.Update(updatedOrder);
    }

    /// <summary>
    /// Deletes an order from the system, only if it is not associated with any delivery records.
    /// </summary>
    /// <param name="orderId">The ID of the order to delete.</param>
    /// <exception cref="InvalidOperationException">If the order is not found or is associated with deliveries.</exception>
    internal static void DeleteOrder(int orderId)
    {
        // check for existing deliveries
        if (s_dal.Delivery.ReadAll(d => d.OrderId == orderId).Any())
        {
            throw new BO.BlCannotDeleteException($"Cannot delete Order {orderId}: order is associated with existing deliveries (history).");
        }

        // attempt deletion
        try
        {
            s_dal.Order.Delete(orderId);
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException($"Order with ID {orderId} does not exist and cannot be deleted.", ex);
        }
    }

    /// <summary>
    /// Helper method to calculates the estimated duration of a delivery for an active order using courier type and routing service.
    /// </summary>
    /// <param name="doDelivery">The DO.Delivery entity which is currently in progress.</param>
    /// <param name="doOrder">The target DO.Order entity.</param>
    /// <returns>The estimated duration (TimeSpan) of the delivery from company to destination.</returns>
    /// <exception cref="BO.BlDoesNotExistException">Thrown if courier or order are not found.</exception>
    /// <exception cref="BO.BlInvalidOperationException">Thrown if routing service fails.</exception>
    internal static TimeSpan CalculateEstimatedDeliveryDuration(DO.Delivery doDelivery, DO.Order doOrder)
    {
        // find the courier details
        DO.Courier doCourier;
        try
        {
            doCourier = s_dal.Courier.Read(doDelivery.CourierId)!;
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException($"Courier ID {doDelivery.CourierId} not found for ongoing delivery.", ex);
        }

        // get company coordinates
        (double companyLat, double companyLon) = GetCompanyCoordinates();
        BO.DeliveryType deliveryType = (BO.DeliveryType)doCourier.TypeOfDelivery; // map courier type to delivery type

        // Calculate actual distance/time using routing service
        (double actualDistance, TimeSpan estimatedTime)? routingResult = Tools.GetActualDistanceAndEstimatedTimeSync(
            companyLat,
            companyLon,
            doOrder.Latitude,
            doOrder.Longitude,
            deliveryType
        );

        if (!routingResult.HasValue)
        {
            // routing service failed
            throw new BO.BlInvalidOperationException($"Routing service failed to estimate time for order {doOrder.Id} and courier {doCourier.Id}.");
        }

        // Return the estimated time
        return routingResult.Value.estimatedTime;
    }

    /// <summary>
    /// Calculates the maximum allowed delivery time for a given order.
    /// </summary>
    /// <param name="orderId">The ID of the order.</param>
    /// <returns>DateTime representing the maximum end time.</returns>
    /// <exception cref="InvalidOperationException">If the order is not found.</exception>
    internal static DateTime CalculateMaxDeliveryTime(int orderId)
    {
        // get existing order
        DO.Order doOrder = GetExistingOrder(orderId);

        // get max delivery range from config
        TimeSpan maxTimeSpan = s_dal.Config.MaxDeliveryRange;

        // calculate maximum delivery time (opening time + max range)
        return doOrder.OrderOpeningTime.Add(maxTimeSpan);
    }

    /// <summary>
    /// Gets the last delivery end time for a given order.
    /// </summary>
    /// <param name="orderId">The ID of the order.</param>
    /// <returns>DateTime representing the last delivery end time.</returns>
    /// <exception cref="InvalidOperationException">If there are no closed deliveries for this order.</exception>
    internal static DateTime GetLastDeliveryEndTime(int orderId)
    {
        var closedDeliveries = s_dal.Delivery.ReadAll(d => d.OrderId == orderId && d.DeliveryEndTime.HasValue);

        if (!closedDeliveries.Any())
        {
            throw new BO.BlInvalidOperationException($"Order ID={orderId} has no closed deliveries.");
        }

        return closedDeliveries.Max(d => d.DeliveryEndTime!.Value);
    }

    /// <summary>
    /// Calculates the schedule status (OnTime/InRisk/Late) of the order.
    /// </summary>
    /// <param name="orderId">The ID of the order.</param>
    /// <returns>BO.ScheduleStatus</returns>
    internal static BO.ScheduleStatus CalculateScheduleStatus(int orderId)
    {
        // first read: ensure order exists
        s_dal.Order.Read(orderId);

        // check for current open delivery
        DO.Delivery? currentDelivery = s_dal.Delivery.ReadAll(d => d.OrderId == orderId && d.DeliveryEndTime == null).FirstOrDefault();

        DateTime maxDeliveryTime = CalculateMaxDeliveryTime(orderId);
        DateTime now = AdminManager.Now; // use current clock time
        TimeSpan riskTimeSpan = s_dal.Config.RiskRange; // risk time span from config

        // check if order is closed or open
        if (currentDelivery == null && s_dal.Delivery.ReadAll(d => d.OrderId == orderId && d.DeliveryEndTime.HasValue).Any())
        {
            DateTime timeOfEnd = GetLastDeliveryEndTime(orderId);

            if (timeOfEnd > maxDeliveryTime)
            {
                return BO.ScheduleStatus.Late; // provided late
            }
            return BO.ScheduleStatus.OnTime; // provided on time
        }
        //if order is still open
        else
        {
            TimeSpan timeRemaining = maxDeliveryTime - now;

            if (timeRemaining.TotalSeconds <= 0)
            {
                return BO.ScheduleStatus.Late; // Exceeded maximum delivery time
            }
            if (timeRemaining <= riskTimeSpan)
            {
                return BO.ScheduleStatus.InRisk; // in risk of being late
            }
            return BO.ScheduleStatus.OnTime; // there is sufficient time remaining
        }
    }

    /// <summary>
    /// Calculates the current status of the order based on its delivery records.
    /// </summary>
    /// <param name="orderId"></param>
    /// <returns></returns>
    internal static BO.OrderStatus CalculateOrderStatus(int orderId)
    {
        // first read: ensure order exists
        s_dal.Order.Read(orderId);

        // find current open delivery
        DO.Delivery? currentDelivery = s_dal.Delivery.ReadAll(d => d.OrderId == orderId && d.DeliveryEndTime == null).FirstOrDefault();
        if (currentDelivery != null)
        {
            return BO.OrderStatus.InProgress; // there is an ongoing delivery
        }

        // find last closed delivery
        DO.Delivery? lastClosedDelivery = s_dal.Delivery.ReadAll(d => d.OrderId == orderId && d.DeliveryEndTime.HasValue)
                                            .OrderByDescending(d => d.DeliveryEndTime)
                                            .FirstOrDefault();

        // determine status based on last closed delivery
        if (lastClosedDelivery == null)
        {
            return BO.OrderStatus.Open; // no deliveries yet- order is open
        }

        switch (lastClosedDelivery.OrderClosedStatus)
        {
            case DO.OrderEndStatus.Delivered:
                return BO.OrderStatus.Delivered;
            case DO.OrderEndStatus.Refused:
                return BO.OrderStatus.Refused;
            case DO.OrderEndStatus.Cancelled:
                return BO.OrderStatus.Cancelled;

            case DO.OrderEndStatus.InviterNotFound:
            case DO.OrderEndStatus.Failed:
                // These statuses imply the order is still open for future delivery attempts
                return BO.OrderStatus.Open;
            default:
                return BO.OrderStatus.Open;
        }
    }

    /// <summary>
    /// Maps delivery records for a given order into BO.DeliveryPerOrderInList objects.
    /// </summary>
    internal static IEnumerable<BO.DeliveryPerOrderInList> MapDeliveryListForOrder(int orderId)
    {
        var closedDeliveries = s_dal.Delivery.ReadAll(d => d.OrderId == orderId && d.DeliveryEndTime.HasValue);

        return closedDeliveries.Select(doDelivery =>
        {
            // Get courier details
            DO.Courier doCourier = s_dal.Courier.Read(doDelivery.CourierId)!;

            // calculate TotalHandlingDuration
            TimeSpan totalHandlingDuration = doDelivery.DeliveryEndTime!.Value - doDelivery.DeliveryStartTime;

            return new BO.DeliveryPerOrderInList
            {
                Id = doDelivery.Id,
                CourierId = doDelivery.CourierId,
                Name = doCourier.Name,
                TypeOfDelivery = (BO.DeliveryType)doCourier.TypeOfDelivery,
                DeliveryStartTime = doDelivery.DeliveryStartTime,
                OrderClosedStatus = (BO.OrderEndStatus?)doDelivery.OrderClosedStatus, // is the cast safe?
                DeliveryEndTime = doDelivery.DeliveryEndTime,
                ActualDistance = doDelivery.ActualDistance,
                TotalHandlingDuration = totalHandlingDuration
            };
        });
    }

    /// <summary>
    /// helper method to get existing order or throw exception if not found.
    /// </summary>
    internal static DO.Order GetExistingOrder(int orderId)
    {
        try
        {
            return s_dal.Order.Read(orderId)!;
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException($"Order with ID {orderId} does not exist.", ex);
        }
    }

    /// <summary>
    /// Asserts that the requesting user has permission to read the full order details.
    /// Permissions: Admin, Courier assigned to the open delivery, or the Customer who placed the order.
    /// </summary>
    /// <remarks>
    /// Note: Customer check is commented out as DO.Order structure is missing the CustomerId field.
    /// </remarks>
    internal static void AssertReadAuthorization(int requestingUserId, int orderId)
    {
        DO.Order doOrder = GetExistingOrder(orderId);

        // check permissions in order
        try
        {
            AdminManager.AssertAdmin(requestingUserId); // if succeeds, user is admin
            return; // admin has access
        }
        catch (BO.BlNotAuthorizedException)
        {
            // user is not admin, continue to other checks.
        }

        // Check if the user is the assigned courier
        // Look for an active delivery (not yet closed) for this order.
        // If the status is "In Progress", it means there is an open delivery.
        DO.Delivery? openDelivery = s_dal.Delivery.ReadAll(d => d.OrderId == doOrder.Id && d.DeliveryEndTime == null).FirstOrDefault();

        // If there is an open delivery and requestingUserId is the assigned courier
        if (openDelivery != null && openDelivery.CourierId == requestingUserId)
        {
            return;
        }

        throw new BO.BlNotAuthorizedException($"User ID {requestingUserId} is not authorized to view Order ID {doOrder.Id}.");
    }

    /// <summary>
    /// helper method to validate order input data.
    /// </summary>
    private static void AssertOrderInputValidity(BO.Order order)
    {
        if (string.IsNullOrEmpty(order.CustomerName) || order.CustomerName.Length < 2)
            throw new BO.BlInvalidDataException("Customer name must contain at least 2 characters.");
        if (string.IsNullOrEmpty(order.CustomerPhone) || order.CustomerPhone.Length != 10 || !order.CustomerPhone.All(char.IsDigit))
            throw new BO.BlInvalidDataException("Phone number is invalid.");
        if (string.IsNullOrWhiteSpace(order.Address))
            throw new BO.BlInvalidDataException("Delivery address cannot be empty.");
    }

    /// <summary>
    /// helper method to get company coordinates from config.
    /// </summary>
    internal static (double Latitude, double Longitude) GetCompanyCoordinates()
    {
        // try to get from dal config
        double? lat = s_dal.Config.CompenyLatitude;
        double? lon = s_dal.Config.CompenyLongitude;

        // if not set, return default hardcoded values
        if (lat == null || lon == null || lat == 0 || lon == 0)
        {
            return (32.0641632, 34.7692375); // Default company coordinates
        }

        return (lat.Value, lon.Value);
    }

    /// <summary>
    /// Helper method to calculates an array representing the summary quantities of orders, 
    /// grouped by a combination of OrderStatus and ScheduleStatus.
    /// </summary>
    /// <returns>An array of integers where the index corresponds to the combined status.</returns>
    internal static int[] GetOrderSummaryQuantities()
    {
        // get all order IDs
        var allOrderIds = s_dal.Order.ReadAll().Select(o => o.Id);

        // define the size of the returned array: number of status types * number of schedule status types
        int orderStatusCount = Enum.GetNames(typeof(BO.OrderStatus)).Length;
        int scheduleStatusCount = Enum.GetNames(typeof(BO.ScheduleStatus)).Length;
        int totalSummarySize = orderStatusCount * scheduleStatusCount;

        // create an array initialized with zeros
        int[] summaryArray = new int[totalSummarySize];

        // group orders by combined status key and count them
        var groupedResults = allOrderIds.GroupBy(orderId =>
        {
            // calculate individual statuses
            BO.OrderStatus status = CalculateOrderStatus(orderId);
            BO.ScheduleStatus scheduleStatus = CalculateScheduleStatus(orderId);

            // combine into a single key for grouping
            // the key is the index in the final array: (Status value) + (ScheduleStatus value * number of Status types)
            int combinedKey = (int)status + ((int)scheduleStatus * orderStatusCount);

            return combinedKey;
        })
        .Select(g => new { CombinedKey = g.Key, Count = g.Count() });

        // populate the summary array with counts
        foreach (var result in groupedResults)
        {
            // safety check for array bounds
            if (result.CombinedKey >= 0 && result.CombinedKey < totalSummarySize)
            {
                summaryArray[result.CombinedKey] = result.Count;
            }
        }

        return summaryArray;
    }

    /// <summary>
    /// Sorts a collection of orders based on the specified field.
    /// </summary>
    internal static IEnumerable<BO.OrderInList> SortOrdersBy(IEnumerable<BO.OrderInList> orders, BO.OrderFieldSort sortBy)
    {
        return sortBy switch
        {
            BO.OrderFieldSort.Id => orders.OrderBy(o => o.OrderId),
            BO.OrderFieldSort.StatusOfOrder => orders.OrderBy(o => o.StatusOfOrder),
            BO.OrderFieldSort.TimeLinessStatus => orders.OrderBy(o => o.TimeLinessStatus),
            BO.OrderFieldSort.OrderOpeningTime => orders.OrderBy(o => o.OrderOpeningTime),
            BO.OrderFieldSort.MaxDeliveryTime => orders.OrderBy(o => o.RemainingTime),
            BO.OrderFieldSort.AirDistance => orders.OrderBy(o => o.AirDistance),

            // default case: sort by StatusOfOrder
            _ => orders.OrderBy(o => o.StatusOfOrder)
        };
    }

    /// <summary>
    /// Filters a collection of orders based on the specified OrderFieldSort and object value.
    /// This method handles casting the filter value based on the required property type.
    /// </summary>
    internal static IEnumerable<BO.OrderInList> FilterOrdersBy(IEnumerable<BO.OrderInList> orders, BO.OrderFieldSort filterBy, object filterValue)
    {
        // if no filter value is provided, return the original collection
        if (filterValue == null)
        {
            return orders;
        }

        // perform filtering based on the specified field
        return filterBy switch
        {
            // fields that require equality filtering (ID, Status, TimeLinessStatus)
            BO.OrderFieldSort.Id => orders.Where(o =>
                (filterValue is int intId && o.OrderId == intId) ||
                (filterValue is string strId && int.TryParse(strId, out int parsedId) && o.OrderId == parsedId)
            ),

            BO.OrderFieldSort.StatusOfOrder => orders.Where(o =>
                (filterValue is BO.OrderStatus status && o.StatusOfOrder == status) ||
                (filterValue is int intStatus && o.StatusOfOrder == (BO.OrderStatus)intStatus) ||
                (filterValue is string strStatus && Enum.TryParse(strStatus, true, out BO.OrderStatus parsedStatus) && o.StatusOfOrder == parsedStatus)
            ),

            BO.OrderFieldSort.TimeLinessStatus => orders.Where(o =>
                (filterValue is BO.ScheduleStatus status && o.TimeLinessStatus == status) ||
                (filterValue is int intStatus && o.TimeLinessStatus == (BO.ScheduleStatus)intStatus) ||
                (filterValue is string strStatus && Enum.TryParse(strStatus, true, out BO.ScheduleStatus parsedStatus)) && o.TimeLinessStatus == parsedStatus
            ),

            // fields that require range filtering (AirDistance, OrderOpeningTime, MaxDeliveryTime)
            BO.OrderFieldSort.AirDistance => orders.Where(o =>
            {
                double? filterD = filterValue is double d ? d : filterValue is string s && double.TryParse(s, out double p) ? p : (double?)null;

                // filtering will only be applied if the value is valid
                return filterD.HasValue && o.AirDistance <= filterD.Value;
            }),

            // fields that require date filtering (OrderOpeningTime): use a 24-hour range
            // and also for MaxDeliveryTime (assuming filtering by the target date only)
            BO.OrderFieldSort.OrderOpeningTime => orders.Where(o =>
            {
                DateTime? filterDT = filterValue is DateTime dt ? dt : (DateTime?)null;

                if (filterDT.HasValue)
                {
                    // creating a 24-hour range for the provided date
                    DateTime startOfDay = filterDT.Value.Date; // today 00:00:00
                    DateTime endOfNextDay = filterDT.Value.Date.AddDays(1); // tomorrow 00:00:00

                    // checking range: >= start of day, and < 00:00:00 of tomorrow
                    return o.OrderOpeningTime >= startOfDay && o.OrderOpeningTime < endOfNextDay;
                }
                return false;
            }),

            // fields that require date filtering (MaxDeliveryTime): use a 24-hour range
            BO.OrderFieldSort.MaxDeliveryTime => orders.Where(o =>
            {
                DateTime? filterDT = filterValue is DateTime dt ? dt : (DateTime?)null;

                if (filterDT.HasValue)
                {
                    // creating a 24-hour range for the provided date
                    DateTime startOfDay = filterDT.Value.Date; // today 00:00:00
                    DateTime endOfNextDay = filterDT.Value.Date.AddDays(1); // tomorrow 00:00:00

                    // checking range: MaxDeliveryTime must be within the date range
                    return o.MaxDeliveryTime >= startOfDay && o.MaxDeliveryTime < endOfNextDay;
                }
                return false;
            }),

            // default case: throw exception for unsupported filter
            _ => throw new BO.BlInvalidDataException($"Filtering by {filterBy} is not supported or the filter value is invalid.")
        };
    }

    /// <summary>
    /// Cancels the order by creating a dummy delivery (if Open) or updating the open delivery (if InProgress).
    /// </summary>
    internal static void CancelOrder(int orderId)
    {
        // retrieve existing order and current status
        DO.Order doOrder = GetExistingOrder(orderId);
        BO.OrderStatus currentStatus = CalculateOrderStatus(orderId);

        DateTime cancellationTime = AdminManager.Now;

        // logic check: cannot cancel if already Delivered, Refused, or Cancelled
        if (currentStatus == BO.OrderStatus.Delivered || currentStatus == BO.OrderStatus.Refused || currentStatus == BO.OrderStatus.Cancelled)
        {
            throw new BO.BlInvalidOperationException($"Order {orderId} cannot be cancelled because its current status is {currentStatus}.");
        }

        // handle cancellation based on current status
        if (currentStatus == BO.OrderStatus.Open)
        {
            // open order: create a dummy delivery record to mark cancellation
            DO.Delivery cancelledDelivery = new DO.Delivery(
                Id: default, // get new ID from DAL
                OrderId: orderId,
                CourierId: 0,
                TypeOfOrder: doOrder.TypeOfOrder,
                DeliveryStartTime: cancellationTime,
                ActualDistance: 0,
                OrderClosedStatus: (DO.OrderEndStatus)BO.OrderEndStatus.Cancelled,
                DeliveryEndTime: cancellationTime 
            );
            s_dal.Delivery.Create(cancelledDelivery);
        }
        else if (currentStatus == BO.OrderStatus.InProgress)
        {
            // in-progress order: update the existing open delivery to mark cancellation
            DO.Delivery openDelivery = s_dal.Delivery.ReadAll(d => d.OrderId == orderId && d.DeliveryEndTime == null)
                .FirstOrDefault() ?? throw new BO.BlInvalidOperationException("Order is InProgress but no open delivery record was found.");

            // Update the delivery
            s_dal.Delivery.Update(openDelivery with
            {
                OrderClosedStatus = (DO.OrderEndStatus)BO.OrderEndStatus.Cancelled,
                DeliveryEndTime = cancellationTime
            });
        }
    }
}
