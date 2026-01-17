namespace BlApi;

/// <summary>
/// Defines the operations available for managing order entities within the system.
/// </summary>
/// <remarks>This interface provides methods for creating, reading, updating, deleting, and managing orders.
public interface IOrder : IObservable
{
    void Create(int requestingUserId, BO.Order boOrder);
    BO.Order Read(int requestingUserId, int orderId);
    IEnumerable<BO.OrderInList> ReadAll(int requestingUserId, BO.OrderFieldSort? sortBy = null, BO.OrderFieldSort? filterBy = null, object? filterValue = null);
    void Update(int requestingUserId, BO.Order boOrder);
    void Delete(int requestingUserId, int orderId);
    void Cancel(int requestingUserId, int orderId);
    int[] GetOrderSummaryQuantities(int requestingUserId);
    void CompleteDelivery(int requestingUserId, int courierId, int deliveryId, BO.OrderEndStatus status);
    void ChooseOrder(int requestingUserId, int courierId, int orderId);
    IEnumerable<BO.ClosedDeliveryInList> GetClosedDeliveriesForCourier(int requestingUserId, int courierId, BO.OrderType? filterByType = null, BO.ClosedDeliveryFieldSort? sortBy = null);
    IEnumerable<BO.OpenOrderInList> ReadAllOpenOrders(int requestingUserId, int courierId, BO.OrderType? filterByType = null, BO.OpenOrderFieldSort? sortBy = null);
}