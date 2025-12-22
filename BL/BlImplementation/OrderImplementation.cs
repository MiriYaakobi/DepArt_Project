namespace BlImplementation;
using BLApi;
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
        // check authorization - only Admin can cancel orders
        AdminManager.AssertAdmin(requestingUserId);

        // Call Manager to perform complex logic
        try
        {
            OrderManager.CancelOrder(orderId);
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
    }

    /// <summary>
    /// completes an ongoing delivery by a specific courier.
    /// </summary>
    /// <param name="requestingUserId"></param>
    /// <param name="courierId"></param>
    /// <param name="deliveryId"></param>
    public void CompleteDelivery(int requestingUserId, int courierId, int deliveryId)
    {
        // access control: only Admin or the courier himself can complete a delivery
        AdminManager.AssertAdminOrSelf(requestingUserId, courierId);

        // call to DeliveryManager to complete the delivery
        DeliveryManager.CompleteDeliveryUpdate(courierId, deliveryId);
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
            // OrderManager.UpdateOrder handles existence check, status check, and the update logic
            OrderManager.UpdateOrder(boOrder);
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
}