namespace BlImplementation;
using BLApi;
using BO;
using System;
using Helpers;

internal class OrderImplementation : IOrder
{
    /// <summary>
    /// Cancels an existing order if it is Open or InProgress. Cancellation creates a dummy delivery 
    /// or updates the current open delivery status to Cancelled.
    /// </summary>
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
    /// Assigns an 'Open' order to a specific courier, creating a new Delivery record in the system.
    /// </summary>
    /// <param name="requestingUserId">The ID of the user attempting the operation (Admin or Courier).</param>
    /// <param name="courierId">The ID of the courier selected to handle the delivery.</param>
    /// <param name="orderId">The ID of the order to be assigned.</param>
    /// <exception cref="BlNotAuthorizedException">Thrown if the requesting user is neither the Admin nor the target Courier.</exception>
    /// <exception cref="BlDoesNotExistException">Thrown if the Order or Courier is not found.</exception>
    /// <exception cref="BlInvalidOperationException">Thrown if the Order is not 'Open' or the Courier is inactive.</exception>
    public void ChooseOrder(int requestingUserId, int courierId, int orderId)
    {
        //Only Admin or the Courier himself can assign the order.
        AdminManager.AssertAdminOrSelf(requestingUserId, courierId);

        // order existence and status check
        BO.Order boOrder;
        try
        {
            // ReadOrder handles DAL-level existence check and maps the full BO entity
            boOrder = OrderManager.ReadOrder(orderId);
        }
        catch (BlDoesNotExistException ex)
        {
            // Translates exception if the order does not exist
            throw new BlDoesNotExistException($"Order ID {orderId} was not found.", ex);
        }

        // check if the order is not in 'Open' status, it cannot be chosen.
        if (boOrder.StatusOfOrder != BO.OrderStatus.Open)
        {
            throw new BlInvalidOperationException($"Order {orderId} cannot be chosen: current status is {boOrder.StatusOfOrder}. Only 'Open' orders are eligible for assignment.");
        }

        // courier existence and status check
        BO.Courier boCourier;
        try
        {
            // ReadCourier handles DAL-level existence check and maps the full BO entity
            boCourier = CourierManager.ReadCourier(courierId);
        }
        catch (BlDoesNotExistException ex)
        {
            // Translates exception if the courier does not exist
            throw new BlDoesNotExistException($"Courier ID {courierId} was not found.", ex);
        }

        // Courier must be active to accept a new delivery.
        if (!boCourier.IsActive)
        {
            throw new BlInvalidOperationException($"Courier {courierId} is currently inactive and cannot accept new orders.");
        }

        // All validations passed, proceed to create the Delivery record.
        // The helper handles Routing, Geocoding, and DO.Delivery creation using AdminManager.Now.
        try
        {
            // The helper handles Routing, Geocoding, and DO.Delivery creation using AdminManager.Now.
            // הערה: נשתמש ב-DeliveryManager כפי שתכננו.
            DeliveryManager.CreateNewDeliveryForOrder(orderId, courierId, boOrder.Latitude, boOrder.Longitude, boCourier.TypeOfDelivery);
        }
        catch (InvalidOperationException ex)
        {
            // translate any internal exception to BL exception
            throw new BO.BlInvalidOperationException($"Failed to assign order {orderId} to courier {courierId}.", ex);
        }
    }

    /// <summary>
    /// Updates a delivery record to 'Delivered' status upon successful completion by the courier.
    /// </summary>
    public void CompleteDelivery(int requestingUserId, int courierId, int deliveryId, double endLat, double endLon)
    {
        // control access: only the assigned courier can report completion
        if (requestingUserId != courierId)
        {
            throw new BO.BlNotAuthorizedException($"User ID {requestingUserId} is not authorized. Only courier {courierId} can report completion for this delivery.");
        }

        // call to Manager
        try
        {
            // DeliveryManager handles:
            // - Existence check for the delivery.
            // - Status check to ensure it's open.
            // - Update of the delivery record.
            DeliveryManager.CompleteDeliveryUpdate(deliveryId, courierId, BO.OrderEndStatus.Delivered, endLat, endLon);
        }
        catch (BO.BlDoesNotExistException)
        {
            throw; // thrown if delivery does not exist
        }
        catch (BO.BlInvalidOperationException)
        {
            throw; // thrown if delivery is already closed or courier does not match
        }
    }

    /// <summary>
    /// Creates a new order in the system after validating input and geocoding the address.
    /// </summary>
    /// <param name="requestingUserId">The ID of the user requesting the operation.</param>
    /// <param name="boOrder">The BO.Order object to create.</param>
    /// <exception cref="BO.BlInvalidDataException">If input data or the address is invalid.</exception>
    public void Create(int requestingUserId, BO.Order boOrder)
    {
        // call to Manager
        try
        {
            // OrderManager handles:
            // - Input validation (AssertOrderInputValidity).
            // - Geocoding (synchronous network call).
            // - Creating the DO.Order and adding it to the DAL.
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
    /// Deletes an order from the system. (Intended for use in testing only).
    /// </summary>
    /// <remarks>The business logic dictates that orders should not be permanently deleted in the live system;
    /// this method throws a BlInvalidOperationException to enforce this policy.</remarks>
    public void Delete(int requestingUserId, int orderId)
    {
        // justification: only Admin can attempt deletion
        AdminManager.AssertAdmin(requestingUserId);

        // conform to business logic: orders cannot be deleted, only cancelled
        throw new BO.BlInvalidOperationException($"Order ID {orderId} cannot be deleted from the system due to business logic (only cancellation is allowed).");

        /* // אם היינו צריכים לממש מחיקה אמיתית, היינו משתמשים בלוגיקה הבאה:
        // try
        // {
        //     OrderManager.DeleteOrder(orderId);
        // }
        // catch (BO.BlDoesNotExistException)
        // {
        //     throw;
        // }
        // catch (BO.BlInvalidOperationException ex)
        // {
        //     throw; // נזרק אם יש משלוחים קשורים
        // }
        */
    }

    /// <summary>
    /// Retrieves a list of closed deliveries for a specific courier, with optional filtering and sorting.
    /// </summary>
    /// <param name="requestingUserId"></param>
    /// <param name="courierId"></param>
    /// <param name="filterByType"></param>
    /// <param name="sortBy"></param>
    /// <returns></returns>
    public IEnumerable<BO.ClosedDeliveryInList> GetClosedDeliveriesForCourier(int requestingUserId, int courierId, BO.OrderType? filterByType = null, BO.ClosedDeliveryFieldSort? sortBy = null)
    {
        // 1. בקרת גישה: רק Admin או השליח עצמו רשאי לצפות בהיסטוריה שלו
        AdminManager.AssertAdminOrSelf(requestingUserId, courierId);

        // 2. קריאה ל-Manager: קבלת הרשימה הגולמית והממופה
        IEnumerable<BO.ClosedDeliveryInList> closedDeliveries = DeliveryManager.GetClosedDeliveriesForCourier(courierId, filterByType);

        // 3. מימוש המיון (Sort)
        if (sortBy.HasValue)
        {
            // המיון יבוצע במתודת עזר ב-DeliveryManager
            closedDeliveries = DeliveryManager.SortClosedDeliveries(closedDeliveries, sortBy.Value);
        }
        else
        {
            // ברירת מחדל: מיון לפי סוג סיום משלוח סטטוס עמידה בזמנים (OrderClosedStatus)
            closedDeliveries = closedDeliveries.OrderBy(d => d.OrderClosedStatus);
        }

        return closedDeliveries;
    }

    public int[] GetOrderSummaryQuantities(int requestingUserId)
    {
        // ensure only Admin can access this summary data
        AdminManager.AssertAdmin(requestingUserId);

        // call the OrderManager to get the summary quantities
        return OrderManager.GetOrderSummaryQuantities();
    }

    /// <summary>
    /// Retrieves the full logical details of a single order.
    /// </summary>
    /// <param name="requestingUserId">The ID of the user requesting the operation.</param>
    /// <param name="orderId">The ID of the order to retrieve.</param>
    /// <returns>A full BO.Order object.</returns>
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
    /// Reads all orders with optional sorting and filtering.
    /// </summary>
    /// <param name="requestingUserId"></param>
    /// <param name="sortBy"></param>
    /// <param name="filterBy"></param>
    /// <param name="filterValue"></param>
    /// <returns></returns>
    public IEnumerable<BO.OrderInList> ReadAll(int requestingUserId, BO.OrderFieldSort? sortBy = null, BO.OrderFieldSort? filterBy = null, object? filterValue = null)
    {
        AdminManager.AssertAdmin(requestingUserId);

        IEnumerable<BO.OrderInList> orders = OrderManager.ReadAllOrders();

        if (filterBy.HasValue && filterValue != null)
        {
            // reference to the helper method for filtering
            orders = OrderManager.FilterOrdersBy(orders, filterBy.Value, filterValue);
        }

        // sorting
        if (sortBy.HasValue)
        {
            orders = OrderManager.SortOrdersBy(orders, sortBy.Value);
        }
        else
        {
            // default sort by StatusOfOrder
            orders = orders.OrderBy(o => o.StatusOfOrder);
        }

        return orders;
    }

    /// <summary>
    /// Retrieves a filtered and sorted list of 'Open' orders that are available for a specific courier to choose.
    /// The list is restricted by the courier's maximum allowed distance.
    /// </summary>
    public IEnumerable<BO.OpenOrderInList> ReadAllOpenOrders(int requestingUserId, int courierId, BO.OrderType? filterByType = null, BO.OpenOrderFieldSort? sortBy = null)
    {
        // access control: only Admin or the courier himself can view available open orders
        AdminManager.AssertAdminOrSelf(requestingUserId, courierId);

        // call to Manager: get the raw mapped list
        IEnumerable<BO.OpenOrderInList> openOrders = DeliveryManager.GetAvailableOpenOrders(courierId, filterByType);

        // Implement sorting
        if (sortBy.HasValue)
        {
            // sorting will be done in a helper method in DeliveryManager
            openOrders = DeliveryManager.SortOpenOrders(openOrders, sortBy.Value);
        }
        else
        {
            // default sort by TimeLinessStatus
            openOrders = openOrders.OrderBy(o => o.TimeLinessStatus);
        }

        return openOrders;
    }

    /// <summary>
    /// Updates the changeable details of an existing order.
    /// </summary>
    /// <param name="requestingUserId">The ID of the user requesting the update.</param>
    /// <param name="boOrder">The BO.Order object containing the updated fields.</param>
    /// <exception cref="BO.BlNotAuthorizedException">If the user is not authorized (Admin only).</exception>
    /// <exception cref="BO.BlDoesNotExistException">If the order ID is not found.</exception>
    /// <exception cref="BO.BlInvalidOperationException">If the order is already closed (Delivered/Refused/Cancelled).</exception>
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
}
