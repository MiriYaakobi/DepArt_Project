namespace BLApi;

/// <summary>
/// Provides access to business logic components for managing couriers, orders, and administrative tasks.
/// </summary>
/// <remarks>This interface serves as a central point for accessing various business logic services.
/// Implementations should provide concrete instances of the <see cref="ICourier"/>, <see cref="IOrder"/>, and <see
/// cref="IAdmin"/> interfaces.</remarks>
public interface IBl
{
    ICourier Courier { get; }
    IOrder Order { get; }
    IAdmin Admin { get; }
}
