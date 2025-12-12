using Helpers;
namespace BO;

/// <summary>
/// Represents a closed delivery entry in a list, containing details about the delivery and its status.
/// </summary>
/// <remarks>This class is used to encapsulate information about a delivery that has been completed, including its
/// order details, delivery type, and final status. It is immutable and designed for use in scenarios where delivery
/// information needs to be displayed or logged.</remarks>
public class ClosedDeliveryInList
{
    public int Id { get; init; }
    public int OrderId { get; init; }
    public OrderType TypeOfOrder { get; init; }
    public string? Address { get; init; }
    public DeliveryType TypeOfDelivery { get; init; }
    public double? ActualDistance { get; init; }
    public TimeSpan TotalHandlingDuration { get; init; }
    public OrderEndStatus OrderClosedStatus { get; init; }
    public DateTime? DeliveryEndTime { get; init; }
    public override string ToString() => this.ToStringProperty(); // Uses Helpers.ToStringProperty extension method
}
