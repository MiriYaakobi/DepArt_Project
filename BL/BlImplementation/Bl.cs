using BLApi;

namespace BlImplementation;

/// <summary>
/// a class that implements the IBl interface and provides access to various business logic components
/// </summary>
/// <remarks>
/// each property returns an instance of a specific implementation class for the corresponding interface
/// </remarks>
internal class Bl : IBl
{
    public ICourier Courier { get; } = new CourierImplementation();

    public IOrder Order { get; } = new OrderImplementation();

    public IAdmin Admin { get; } = new AdminImplementation();
}
