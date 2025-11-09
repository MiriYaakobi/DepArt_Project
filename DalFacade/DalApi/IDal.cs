using System.Reflection.Metadata;

namespace DalApi;

/// <summary>
/// <summary>
/// Data Access Layer interface for managing entities.
/// </summary>

public interface IDal
{
    ICourier Couriers { get; }
    IOrder Orders { get; }
    IDelivery Deliveries { get; }
    IConfig Config { get; }

    void ResetDB();
}
