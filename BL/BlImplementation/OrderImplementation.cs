namespace BlImplementation;
using BLApi;
using Helpers;

internal class OrderImplementation : IOrder
{
    public void Cancel(int requestingUserId, int orderId)
    {
        
    }

    public void ChooseOrder(int requestingUserId, int courierId, int orderId)
    {
        throw new NotImplementedException();
    }

    public void CompleteDelivery(int requestingUserId, int courierId, int deliveryId)
    {
        throw new NotImplementedException();
    }

    public void Create(int requestingUserId, BO.Order boOrder)
    {
        throw new NotImplementedException();
    }

    public void Delete(int requestingUserId, int orderId)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<BO.ClosedDeliveryInList> GetClosedDeliveriesForCourier(int requestingUserId, int courierId, BO.OrderType? filterByType = null, BO.ClosedDeliveryFieldSort? sortBy = null)
    {
        throw new NotImplementedException();
    }

    public int[] GetOrderSummaryQuantities(int requestingUserId)
    {
        throw new NotImplementedException();
    }

    public BO.Order Read(int requestingUserId, int orderId)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<BO.OrderInList> ReadAll(int requestingUserId, BO.OrderFieldSort? sortBy = null, BO.OrderType? filterBy = null, object? filterValue = null)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<BO.OpenOrderInList> ReadAllOpenOrders(int requestingUserId, int courierId, BO.OrderType? filterByType = null, BO.OpenOrderFieldSort? sortBy = null)
    {
        throw new NotImplementedException();
    }

    public void Update(int requestingUserId, BO.Order boOrder)
    {
        throw new NotImplementedException();
    }
}
