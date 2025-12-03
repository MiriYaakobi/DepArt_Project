using Helpers;
namespace BO;

/// <summary>
/// Represents an order within a list, including details such as order type, status, and timing information.
/// </summary>
/// <remarks>This class provides properties to access various attributes of an order, such as its unique
/// identifier, type, status, and timing details. It is designed to be immutable after initialization.</remarks>
public class OrderInList
{
    public int? Id { get; init; }
    public int OrderId { get; init; }
    public OrderType TypeOfOrder { get; init; }
    public double AirDistance { get; init; }
    public OrderStatus StatusOfOrder { get; init; }
    public SchedualeStatus TimeLinessStatus { get; init; }
    public TimeSpan RemainingTime { get; init; }
    public TimeSpan TotalHandlingDuration { get; init; }
    public int TotalDeliveries { get; init; }
    public override string ToString() => this.ToStringProperty(); // Uses Helpers.ToStringProperty extension method
}
