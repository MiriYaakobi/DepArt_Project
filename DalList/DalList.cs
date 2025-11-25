namespace Dal;
using DalApi;
using DO;

/// <summary>
/// Implementation of the Data Access Layer (DAL) for in-memory data storage.
/// </summary>
sealed internal class DalList : IDal
{
    /// <summary>
    /// Private constructor to prevent instantiation.
    /// </summary>
    private DalList() { }

    /// <summary>
    /// Gets the singleton instance of the DalList class.
    /// </summary>
    public static DalList Instance => Nested.instance;

    // Nested class to hold the singleton instance
    private static class Nested
    {
        internal static readonly DalList instance = new DalList();
    }

    public ICourier Courier { get; } = new CourierImplementation();

    public IOrder Order { get; } = new OrderImplementation();

    public IDelivery Delivery { get; } = new DeliveryImplementation();

    public IConfig Config { get; } = new ConfigImplementation();

    public void ResetDB()
    {
        Courier.DeleteAll();
        Order.DeleteAll();
        Delivery.DeleteAll();
        Config.Reset();
    }
}
