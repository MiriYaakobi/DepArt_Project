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
    int Id { get; init; }
    int OrderId { get; init; }
    OrderType TypeOfOrder { get; init; }
    string Address { get; init; }
    DeliveryType TypeOfDelivery { get; init; }
    double? ActualDistance { get; init; }
    TimeSpan TotalHandlingDuration { get; init; }
    OrderEndStatus OrderClosedStatus { get; init; }
    public override string ToString() => this.ToStringProperty(); // Uses Helpers.ToStringProperty extension method
}
