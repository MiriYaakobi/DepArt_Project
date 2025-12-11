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
            throw new ArgumentException($"Address '{order.Address}' is invalid or could not be found.");
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
        // use helper to get existing order or throw exception
        DO.Order doOrder = GetExistingOrder(orderId);

        // calculate statuses
        BO.OrderStatus statusOfOrder = CalculateOrderStatus(orderId);
        BO.ScheduleStatus timeLinessStatus = CalculateScheduleStatus(orderId);
        DateTime maxDeliveryTime = CalculateMaxDeliveryTime(orderId);

        // calculate air distance
        double airDistance = Tools.GetAirDistance(
            s_dal.Config.CompenyLatitude ?? 0,
            s_dal.Config.CompenyLongitude ?? 0,
            doOrder.Latitude,
            doOrder.Longitude
        );
        TimeSpan remainingDeliveryTime = maxDeliveryTime - AdminManager.Now;

        // Retrieve shipping details (DeliveryPerOrderInList)
        IEnumerable<BO.DeliveryPerOrderInList> deliveryListCollection = MapDeliveryListForOrder(orderId);
        BO.DeliveryPerOrderInList? deliveryList = deliveryListCollection.FirstOrDefault();

        // map to BO.Order and return
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

            // TO_DO: חישוב ExpectedDeliveryTime (נשאר NULL, נדרשת לוגיקה נוספת מפרק 9)
            ExpectedDeliveryTime = null,

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

            // calculate statuses
            BO.OrderStatus statusOfOrder = CalculateOrderStatus(orderId);
            BO.ScheduleStatus timeLinessStatus = CalculateScheduleStatus(orderId);

            // calculate total deliveries and handling duration
            int totalDeliveries = s_dal.Delivery.ReadAll(d => d.OrderId == orderId).Count();
            TimeSpan totalHandlingDuration = TimeSpan.Zero; // TO_DO: נשאר 0 כיוון שדורש סיכום משלוחים סגורים (פרק 9)

            // calculate remaining time and air distance
            DateTime maxDeliveryTime = CalculateMaxDeliveryTime(orderId);
            TimeSpan remainingTime = maxDeliveryTime - AdminManager.Now;
            double airDistance = Tools.GetAirDistance(
                companyCoords.Latitude,
                companyCoords.Longitude,
                doOrder.Latitude,
                doOrder.Longitude
            );

            return new BO.OrderInList
            {
                OrderId = orderId,
                TypeOfOrder = (BO.OrderType)doOrder.TypeOfOrder,
                AirDistance = airDistance,
                StatusOfOrder = statusOfOrder,
                TimeLinessStatus = timeLinessStatus,
                RemainingTime = remainingTime,
                TotalHandlingDuration = totalHandlingDuration,
                TotalDeliveries = totalDeliveries
            };
        });
        // TO_DO: יש לבצע מיון וסינון מלא בממשק הציבורי. (הערה לפרק 9)
    }

    /// <summary>
    /// Updates an existing order after validating its status and input data.
    /// </summary>
    /// <param name="order"></param>
    /// <exception cref="InvalidOperationException"></exception>
    internal static void UpdateOrder(BO.Order order)
    {
        // check input validity
        DO.Order existingOrder = GetExistingOrder(order.Id);

        // logic check: only Open or InProgress orders can be updated
        BO.OrderStatus currentStatus = CalculateOrderStatus(order.Id);
        if (currentStatus == BO.OrderStatus.Delivered || currentStatus == BO.OrderStatus.Refused || currentStatus == BO.OrderStatus.Cancelled)
        {
            throw new InvalidOperationException($"Cannot update Order {order.Id}. Status is {currentStatus}.");
        }

        // update fields
        DO.Order updatedOrder = existingOrder with
        {
            TypeOfOrder = (DO.OrderType)order.TypeOfOrder,
            Description = order.Description,
            PackageDetails = order.PackageDetails,
            CustomerName = order.CustomerName!,
            CustomerPhone = order.CustomerPhone!
            // שימו לב: Address, Latitude, Longitude, OrderOpeningTime אינם ניתנים לעדכון כאן
        };

        // call DAL to update
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
            throw new InvalidOperationException($"Cannot delete Order {orderId}: order is associated with existing deliveries (history).");
        }

        // attempt deletion
        try
        {
            s_dal.Order.Delete(orderId);
        }
        catch (DO.DalDoesNotExistException)
        {
            throw new InvalidOperationException($"Order with ID {orderId} does not exist and cannot be deleted.");
        }
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
            throw new InvalidOperationException($"Order ID={orderId} has no closed deliveries.");
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
    private static DO.Order GetExistingOrder(int orderId)
    {
        try
        {
            return s_dal.Order.Read(orderId)!;
        }
        catch (DO.DalDoesNotExistException)
        {
            throw new InvalidOperationException($"Order with ID {orderId} does not exist.");
        }
    }

    /// <summary>
    /// helper method to validate order input data.
    /// </summary>
    private static void AssertOrderInputValidity(BO.Order order)
    {
        if (string.IsNullOrEmpty(order.CustomerName) || order.CustomerName.Length < 2)
            throw new ArgumentException("Customer name must contain at least 2 characters.");
        if (string.IsNullOrEmpty(order.CustomerPhone) || order.CustomerPhone.Length != 10 || !order.CustomerPhone.All(char.IsDigit))
            throw new ArgumentException("Phone number is invalid.");
        if (string.IsNullOrWhiteSpace(order.Address))
            throw new ArgumentException("Delivery address cannot be empty.");
    }

    /// <summary>
    /// helper method to get company coordinates from config.
    /// </summary>
    private static (double Latitude, double Longitude) GetCompanyCoordinates()
    {
        // Using null-coalescing operator to provide default values in case of null
        return (s_dal.Config.CompenyLatitude ?? 0, s_dal.Config.CompenyLongitude ?? 0);
    }
}
