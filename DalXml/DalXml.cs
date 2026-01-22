namespace Dal;
using DalApi;
using System.Diagnostics;

sealed internal class DalXml : IDal
{
    /// <summary>
    /// Private constructor to prevent instantiation.
    /// </summary>
    private DalXml() { }

    /// <summary>
    /// Gets the singleton instance of the DalXml class.
    /// </summary>
    public static DalXml Instance => Nested.instance;

    // Nested class to hold the singleton instance
    private static class Nested
    {
        internal static readonly DalXml instance = new DalXml();
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