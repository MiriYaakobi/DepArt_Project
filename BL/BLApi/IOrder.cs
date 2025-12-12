namespace BLApi;

/// <summary>
/// Defines the operations available for managing order entities within the system.
/// </summary>
public interface IOrder
{
    int[] GetOrderSummaryQuantities(int requestingUserId);
    IEnumerable<BO.OrderInList> ReadAll(int requestingUserId, BO.OrderFieldSort? sortBy = null, BO.OrderFieldSort? filterBy = null, object? filterValue = null);
    BO.Order Read(int requestingUserId, int orderId);
    void Update(int requestingUserId, BO.Order boOrder);
    void Cancel(int requestingUserId, int orderId);
    void Delete(int requestingUserId, int orderId);
    void Create(int requestingUserId, BO.Order boOrder);
    void CompleteDelivery(int requestingUserId, int courierId, int deliveryId, double endLat, double endLon);
    void ChooseOrder(int requestingUserId, int courierId, int orderId);
    IEnumerable<BO.ClosedDeliveryInList> GetClosedDeliveriesForCourier(int requestingUserId, int courierId, BO.OrderType? filterByType = null, BO.ClosedDeliveryFieldSort? sortBy = null);
    IEnumerable<BO.OpenOrderInList> ReadAllOpenOrders(int requestingUserId, int courierId, BO.OrderType? filterByType = null, BO.OpenOrderFieldSort? sortBy = null);
}