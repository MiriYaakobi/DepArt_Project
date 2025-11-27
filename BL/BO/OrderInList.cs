using Helpers;
namespace BO;

/// <summary>
/// Represents an order within a list, including details such as order type, status, and timing information.
/// </summary>
/// <remarks>This class provides properties to access various attributes of an order, such as its unique
/// identifier, type, status, and timing details. It is designed to be immutable after initialization.</remarks>
public class OrderInList
{
    int? Id { get; init; }
    int OrderId { get; init; }
    OrderType TypeOfOrder { get; init; }
    double AirDistance { get; init; }
    OrderStatus StatusOfOrder { get; init; }
    SchedualeStatus TimeLinessStatus { get; init; }
    TimeSpan RemainingTime { get; init; }
    TimeSpan TotalHandlingDuration { get; init; }
    int TotalDeliveries { get; init; }
    public override string ToString() => this.ToStringProperty(); // Uses Helpers.ToStringProperty extension method
}
