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
    int Id { get; init; }
    int? CourierId { get; init; }
    string Name { get; init; }
    DeliveryType TypeOfDelivery { get; init; }
    DateTime DeliveryStartTime { get; init; }
    OrderEndStatus? OrderClosedStatus { get; init; }
    DateTime? DeliveryEndTime { get; init; }
    public override string ToString() => this.ToStringProperty(); // Uses Helpers.ToStringProperty extension method
}
