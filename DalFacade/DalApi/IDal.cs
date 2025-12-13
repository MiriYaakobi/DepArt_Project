namespace DalApi;

/// <summary>
/// <summary>
/// Data Access Layer interface for managing entities.
/// </summary>

public interface IDal
{
    ICourier Courier { get; }
    IOrder Order { get; }
    IDelivery Delivery { get; }
    IConfig Config { get; }
    void ResetDB();
}

