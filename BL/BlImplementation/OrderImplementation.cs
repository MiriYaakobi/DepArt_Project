namespace BlImplementation;
using BlApi;
using Helpers;
using System;

/// <summary>
/// a business logic implementation for order-related operations.
/// </summary>
/// <remarks>
/// ensures proper authorization, validation, and error handling.
/// </remarks>
internal class OrderImplementation : IOrder
{
    /// <summary>
    /// cancels an existing order in the system.
    /// </summary>
    /// <param name="requestingUserId"></param>
    /// <param name="orderId"></param>
    /// <exception cref="BO.BlInvalidOperationException"></exception>
    public void Cancel(int requestingUserId, int orderId)
    {
        AdminManager.ThrowOnSimulatorIsRunning();

        // check authorization - only Admin can cancel orders
        AdminManager.AssertAdmin(requestingUserId);

        // Call Manager to perform complex logic
        try
        {
            // find the courier assigned to this order (corrected logic)
            BO.Courier? courierToNotify = null;
            try
            {
                // get the light list of couriers
                var allCouriersList = CourierManager.ReadAllCouriers();

                foreach (var item in allCouriersList)
                {
                    try
                    {
                        // get full courier details
                        BO.Courier c = CourierManager.ReadCourier(item.Id);

                        // check if they have a current order and if it matches the orderId
                        if (c.CurrentOrder != null && c.CurrentOrder.OrderId == orderId)
                        {
                            courierToNotify = c;
                            break;
                        }
                    }
                    catch { continue; }
                }
            }
            catch { }

            OrderManager.CancelOrder(orderId);

            if (courierToNotify != null)
            {
                //string emailToSend = !string.IsNullOrEmpty(courierToNotify.Email) ? courierToNotify.Email : "depart.ilv@gmail.com"; // use courier email if available
                string emailToSend = "depart.ilv@gmail.com"; // for testing purposes, send to our company email

                 EmailService.SendNotification(
                     emailToSend,
                     $"Alert: Order #{orderId} Cancelled",
                     $"Hello {courierToNotify.Name},\n\nThe order #{orderId} assigned to you has been cancelled.\nPlease stop the delivery process.\n\nBest Regards,\nDelivery System"
                 );
            }
        }
        catch (BO.BlDoesNotExistException)
        {
            throw;
        }
        catch (BO.BlInvalidOperationException)
        {
            throw;
        }
        catch (InvalidOperationException ex)
        {
            // translate any other internal exception to BL exception
            throw new BO.BlInvalidOperationException(ex.Message, ex);
        }
    }

    /// <summary>
    /// courier chooses an open order to deliver.
    /// When writing this function, we used AI to ensure that the logic and sorting order were correct.
    /// </summary>
    /// <param name="requestingUserId"></param>
    /// <param name="courierId"></param>
    /// <param name="orderId"></param>
    /// <exception cref="BO.BlInvalidOperationException"></exception>
    public void ChooseOrder(int requestingUserId, int courierId, int orderId)
    {
        AdminManager.ThrowOnSimulatorIsRunning();

        // access control: only Admin or the courier himself can choose an order
        AdminManager.AssertAdminOrSelf(requestingUserId, courierId);

        // validate order status
        BO.Order boOrder = OrderManager.ReadOrder(orderId);

        // only 'Open' orders can be assigned
        if (boOrder.StatusOfOrder != BO.OrderStatus.Open)
            throw new BO.BlInvalidOperationException($"Order {orderId} is not available. Only 'Open' orders can be assigned.");

        // validate courier status
        BO.Courier boCourier = CourierManager.ReadCourier(courierId);

        // only active couriers can choose orders
        if (!boCourier.IsActive)
            throw new BO.BlInvalidOperationException($"Courier {courierId} is inactive.");

        // call to DeliveryManager to create the delivery record
        DeliveryManager.CreateNewDeliveryForOrder(orderId, courierId, boOrder.Latitude, boOrder.Longitude, boCourier.TypeOfDelivery);

        try
        {
            //string courierEmail = !string.IsNullOrEmpty(boCourier.Email) ? boCourier.Email : "depart.ilv@gmail.com"; // use courier email if available
            string courierEmail = "depart.ilv@gmail.com"; // for testing purposes, send to our company email

            string subject = $"New Delivery Assigned! Order #{orderId}";
            string body = $@"Hello {boCourier.Name},

             You have successfully picked up Order #{orderId}.

                📦 Order Details:
                ------------------
                Address: {boOrder.Address}
                Package: {boOrder.PackageDetails}
                Customer: {boOrder.CustomerName}
                Phone: {boOrder.CustomerPhone}

                Navigate safely!
                Delivery System";

            EmailService.SendNotification(courierEmail, subject, body);
        }
        catch
        {
            Console.WriteLine($"Warning: Failed to send email notification.");
        }
    }

    /// <summary>
    /// completes an ongoing delivery by a specific courier.
    /// </summary>
    /// <param name="requestingUserId"></param>
    /// <param name="courierId"></param>
    /// <param name="deliveryId"></param>
    public void CompleteDelivery(int requestingUserId, int courierId, int deliveryId, BO.OrderEndStatus status)
    {
        AdminManager.ThrowOnSimulatorIsRunning();

        // access control: only Admin or the courier himself can complete a delivery
        AdminManager.AssertAdminOrSelf(requestingUserId, courierId);

        // call to DeliveryManager to complete the delivery
        DeliveryManager.CompleteDeliveryUpdate(courierId, deliveryId, status);
    }

    /// <summary>
    /// creates a new order in the system.
    /// </summary>
    /// <param name="requestingUserId"></param>
    /// <param name="boOrder"></param>
    /// <exception cref="BO.BlInvalidDataException"></exception>
    /// <exception cref="BO.BlInvalidOperationException"></exception>
    public void Create(int requestingUserId, BO.Order boOrder)
    {
        AdminManager.ThrowOnSimulatorIsRunning();

        // call to Manager
        try
        {
            // OrderManager.CreateOrder handles:
            OrderManager.CreateOrder(boOrder);
        }
        catch (ArgumentException ex)
        {
            // treatment of invalid input data exceptions
            throw new BO.BlInvalidDataException($"Invalid data provided for order creation: {ex.Message}", ex);
        }
        catch (InvalidOperationException ex)
        {
            // treatment of rare internal errors, e.g., if the DAL fails to create the ID.
            throw new BO.BlInvalidOperationException($"An internal error occurred during order creation.", ex);
        }
    }

    /// <summary>
    /// deletes an existing order from the system.
    /// </summary>
    /// <param name="requestingUserId"></param>
    /// <param name="orderId"></param>
    /// <exception cref="BO.BlInvalidOperationException"></exception>
    public void Delete(int requestingUserId, int orderId)
    {
        AdminManager.ThrowOnSimulatorIsRunning();

        // justification: only Admin can attempt deletion
        AdminManager.AssertAdmin(requestingUserId);

        // conform to business logic: orders cannot be deleted, only cancelled
        throw new BO.BlInvalidOperationException($"Orders cannot be removed from the system.");

    }

    /// <summary>
    /// gets a list of closed deliveries for a specific courier, with optional filtering and sorting.
    /// When writing this function, we used AI to ensure that the logic and sorting order were correct.
    /// </summary>
    /// <param name="requestingUserId"></param>
    /// <param name="courierId"></param>
    /// <param name="filterByType"></param>
    /// <param name="sortBy"></param>
    /// <returns></returns>
    public IEnumerable<BO.ClosedDeliveryInList> GetClosedDeliveriesForCourier(int requestingUserId, int courierId, BO.OrderType? filterByType = null, BO.ClosedDeliveryFieldSort? sortBy = null)
    {
        // access control: only Admin or the courier himself can view closed deliveries
        AdminManager.AssertAdminOrSelf(requestingUserId, courierId);

        // call to Manager: get the raw mapped list
        IEnumerable<BO.ClosedDeliveryInList> closedDeliveries = DeliveryManager.GetClosedDeliveriesForCourier(courierId, filterByType);

        // Implement sorting
        if (sortBy.HasValue)
        {
            // sorting will be done in a helper method in DeliveryManager
            closedDeliveries = DeliveryManager.SortClosedDeliveries(closedDeliveries, sortBy.Value);
        }
        else
        {
            // Default: sort by delivery end status (OrderClosedStatus)
            closedDeliveries = closedDeliveries.OrderBy(d => d.OrderClosedStatus);
        }

        return closedDeliveries;
    }

    /// <summary>
    /// gets summary quantities of orders by status.
    /// </summary>
    /// <param name="requestingUserId"></param>
    /// <returns></returns>
    public int[] GetOrderSummaryQuantities(int requestingUserId)
    {
        // ensure only Admin can access this summary data
        AdminManager.AssertAdmin(requestingUserId);

        // call the OrderManager to get the summary quantities
        return OrderManager.GetOrderSummaryQuantities();
    }

    /// <summary>
    /// gets the details of a specific order by its ID, with authorization checks.
    /// </summary>
    /// <param name="requestingUserId"></param>
    /// <param name="orderId"></param>
    /// <returns></returns>
    /// <exception cref="BO.BlInvalidOperationException"></exception>
    public BO.Order Read(int requestingUserId, int orderId)
    {
        try
        {
            // check authorization first
            OrderManager.AssertReadAuthorization(requestingUserId, orderId);
        }
        // treat specific authorization exceptions
        catch (BO.BlDoesNotExistException)
        {
            throw; // if the order does not exist, propagate the exception
        }
        catch (BO.BlNotAuthorizedException)
        {
            throw; // if the user is not authorized, propagate the exception
        }
        catch (Exception ex)
        {
            // catch any other internal exception that occurred during the authorization check
            throw new BO.BlInvalidOperationException($"An internal error occurred during authorization check for Order ID {orderId}.", ex);
        }

        // read the order details
        try
        {
            return OrderManager.ReadOrder(orderId);
        }
        // catch any other exceptions
        catch (BO.BlDoesNotExistException)
        {
            // This exception will only be caught here if GetExistingOrder inside ReadOrder failed
            throw;
        }
    }

    /// <summary>
    /// gets a list of orders with optional filtering and sorting.
    /// </summary>
    /// <param name="requestingUserId"></param>
    /// <param name="sortBy"></param>
    /// <param name="filterBy"></param>
    /// <param name="filterValue"></param>
    /// <returns></returns>
    public IEnumerable<BO.OrderInList> ReadAll(int requestingUserId, BO.OrderFieldSort? sortBy = null, BO.OrderFieldSort? filterBy = null, object? filterValue = null)
    {
        AdminManager.AssertAdmin(requestingUserId);

        // call to Manager: get the raw mapped list
        IEnumerable<BO.OrderInList> orders = OrderManager.ReadAllOrders();

        if (filterBy.HasValue && filterValue != null)
        {
            // reference to the helper method for filtering
            orders = OrderManager.FilterOrdersBy(orders, filterBy.Value, filterValue);
        }

        // sorting
        if (sortBy.HasValue)
            orders = OrderManager.SortOrdersBy(orders, sortBy.Value);
        else
            // default sort by StatusOfOrder
            orders = orders.OrderBy(o => o.StatusOfOrder);

        return orders;
    }

    /// <summary>
    /// gets all open orders available for a specific courier, with optional filtering and sorting.
    /// </summary>
    /// <param name="requestingUserId"></param>
    /// <param name="courierId"></param>
    /// <param name="filterByType"></param>
    /// <param name="sortBy"></param>
    /// <returns></returns>
    public IEnumerable<BO.OpenOrderInList> ReadAllOpenOrders(int requestingUserId, int courierId, BO.OrderType? filterByType, BO.OpenOrderFieldSort? sortBy)
    {
        // access control: only Admin or the courier himself can view available open orders
        AdminManager.AssertAdminOrSelf(requestingUserId, courierId);

        // call to Manager: get the raw mapped list
        IEnumerable<BO.OpenOrderInList> openOrders = DeliveryManager.GetAvailableOpenOrders(courierId, filterByType);

        // Implement sorting
        if (sortBy.HasValue)
            // sorting will be done in a helper method in DeliveryManager
            openOrders = DeliveryManager.SortOpenOrders(openOrders, sortBy.Value);
        else
            // default sort by TimeLinessStatus
            openOrders = openOrders.OrderByDescending(o => o.TimeLinessStatus);

        return openOrders;
    }

    /// <summary>
    /// updates an existing order in the system.
    /// </summary>
    /// <param name="requestingUserId"></param>
    /// <param name="boOrder"></param>
    /// <exception cref="BO.BlDoesNotExistException"></exception>
    /// <exception cref="BO.BlInvalidOperationException"></exception>
    public void Update(int requestingUserId, BO.Order boOrder)
    {
        // update only allowed by Admin
        AdminManager.AssertAdmin(requestingUserId);

        // call to Manager
        try
        {
            BO.Courier? courierToNotify = null;

            // If the order status is changing to Cancelled, find the assigned courier
            if (boOrder.StatusOfOrder == BO.OrderStatus.Cancelled)
            {
                try
                {
                    var allCouriersList = CourierManager.ReadAllCouriers();
                    foreach (var item in allCouriersList)
                    {
                        try
                        {
                            BO.Courier c = CourierManager.ReadCourier(item.Id);
                            // check by OrderId (or Id if that's how it's named in OrderInProgress)
                            if (c.CurrentOrder != null && c.CurrentOrder.OrderId == boOrder.Id)
                            {
                                courierToNotify = c;
                                break;
                            }
                        }
                        catch { continue; }
                    }
                }
                catch { }
            }

            // OrderManager.UpdateOrder handles existence check, status check, and the update logic
            OrderManager.UpdateOrder(boOrder);

            if (courierToNotify != null)
            {
                //string emailToSend = !string.IsNullOrEmpty(courierToNotify.Email) ? courierToNotify.Email : "depart.ilv@gmail.com"; // use courier email if available
                string emailToSend = "depart.ilv@gmail.com"; // for testing purposes, send our company email

                EmailService.SendNotification(
                     emailToSend,
                     $"Update: Order #{boOrder.Id} Cancelled",
                     $"Hello {courierToNotify.Name},\n\nThe order #{boOrder.Id} assigned to you has been cancelled by the admin.\nYou do not need to deliver it.\n\nBest Regards,\nDelivery System"
                );

            }
        }
        // exception handling
        catch (DO.DalDoesNotExistException ex)
        {
            // convert DAL exception to BL exception
            throw new BO.BlDoesNotExistException($"Order with ID {boOrder.Id} does not exist.", ex);
        }
        catch (InvalidOperationException ex)
        {
            // convert internal logic exception (e.g., trying to update closed order) to BL exception
            throw new BO.BlInvalidOperationException(ex.Message, ex);
        }
    }

    public void AddObserver(Action listObserver) =>
       OrderManager.Observers.AddListObserver(listObserver);

    public void AddObserver(int id, Action observer) =>
        OrderManager.Observers.AddObserver(id, observer);

    public void RemoveObserver(Action listObserver) =>
        OrderManager.Observers.RemoveListObserver(listObserver);

    public void RemoveObserver(int id, Action observer) =>
        OrderManager.Observers.RemoveObserver(id, observer);

    // ----------------------------------------------------------------------
    // גרסאות אסינכרוניות (שלב 7)
    // ----------------------------------------------------------------------

    /// <summary>
    /// ASYNC version: creates a new order in the system.
    /// </summary>
    public async Task CreateAsync(int requestingUserId, BO.Order boOrder)
    {
        AdminManager.ThrowOnSimulatorIsRunning();

        try
        {
            // שימוש בפונקציה האסינכרונית מהמנהל
            await OrderManager.CreateOrderAsync(boOrder);
        }
        catch (ArgumentException ex)
        {
            throw new BO.BlInvalidDataException($"Invalid data provided for order creation: {ex.Message}", ex);
        }
        catch (InvalidOperationException ex)
        {
            throw new BO.BlInvalidOperationException($"An internal error occurred during order creation.", ex);
        }
    }

    /// <summary>
    /// ASYNC version: gets details of a specific order.
    /// </summary>
    public async Task<BO.Order> ReadAsync(int requestingUserId, int orderId)
    {
        try
        {
            // בדיקת הרשאות (נשארת סינכרונית כי היא עובדת מול מסד הנתונים בלבד)
            OrderManager.AssertReadAuthorization(requestingUserId, orderId);
        }
        catch (BO.BlDoesNotExistException) { throw; }
        catch (BO.BlNotAuthorizedException) { throw; }
        catch (Exception ex)
        {
            throw new BO.BlInvalidOperationException($"An internal error occurred during authorization check for Order ID {orderId}.", ex);
        }

        try
        {
            // הקריאה הכבדה (חישוב זמנים) מתבצעת באופן אסינכרוני
            return await OrderManager.ReadOrderAsync(orderId);
        }
        catch (BO.BlDoesNotExistException) { throw; }
    }

    /// <summary>
    /// ASYNC version: courier chooses an order.
    /// </summary>
    public async Task ChooseOrderAsync(int requestingUserId, int courierId, int orderId)
    {
        AdminManager.ThrowOnSimulatorIsRunning();

        // בדיקות מקדימות (נשארות סינכרוניות)
        AdminManager.AssertAdminOrSelf(requestingUserId, courierId);
        BO.Order boOrder = OrderManager.ReadOrder(orderId);

        if (boOrder.StatusOfOrder != BO.OrderStatus.Open)
            throw new BO.BlInvalidOperationException($"Order {orderId} is not available. Only 'Open' orders can be assigned.");

        BO.Courier boCourier = CourierManager.ReadCourier(courierId);
        if (!boCourier.IsActive)
            throw new BO.BlInvalidOperationException($"Courier {courierId} is inactive.");

        // הפעולה הכבדה (יצירת משלוח וחישוב מסלול) מתבצעת אסינכרונית
        await DeliveryManager.CreateNewDeliveryForOrderAsync(orderId, courierId, boOrder.Latitude, boOrder.Longitude, boCourier.TypeOfDelivery);

        // שליחת מייל (נשארת זהה למקור)
        try
        {
            string courierEmail = "depart.ilv@gmail.com";
            string subject = $"New Delivery Assigned! Order #{orderId}";
            string body = $@"Hello {boCourier.Name},

             You have successfully picked up Order #{orderId}.

               📦 Order Details:
               ------------------
               Address: {boOrder.Address}
               Package: {boOrder.PackageDetails}
               Customer: {boOrder.CustomerName}
               Phone: {boOrder.CustomerPhone}

               Navigate safely!
               Delivery System";

            EmailService.SendNotification(courierEmail, subject, body);
        }
        catch
        {
            // התעלמות מכישלון בשליחת מייל
        }
    }
}
