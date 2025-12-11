using Helpers;
namespace BO;

/// <summary>
/// Represents a delivery associated with an order, including details such as the courier, delivery type, and timing.
/// </summary>
/// <remarks>This class provides information about a specific delivery within a list of orders. It includes
/// properties for identifying the delivery,  the courier responsible, and the status and timing of the delivery
/// process.</remarks>
public class DeliveryPerOrderInList
{
    public int Id { get; init; }
    public int? CourierId { get; init; }
    public string? Name { get; init; }
    public DeliveryType TypeOfDelivery { get; init; }
    public DateTime DeliveryStartTime { get; init; }
    public OrderEndStatus? OrderClosedStatus { get; init; }
    public DateTime? DeliveryEndTime { get; init; }
    public double? ActualDistance { get; init; }
    public TimeSpan TotalHandlingDuration { get; init; }
    public override string ToString() => this.ToStringProperty(); // Uses Helpers.ToStringProperty extension method
}
