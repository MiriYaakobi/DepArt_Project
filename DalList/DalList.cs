namespace Dal;
using DalApi;
using DO;

sealed public class DalList : IDal
{
    public ICourier Couriers { get; } = new CourierImplementation();

    public IOrder Orders { get; } = new OrderImplementation();

    public IDelivery Deliveries { get; } = new DeliveryImplementation();

    public IConfig Config { get; } = new ConfigImplementation();

    public void ResetDB()
    {
        Couriers.DeleteAll();
        Orders.DeleteAll();
        Deliveries.DeleteAll();
        Config.Reset();
    }
}
