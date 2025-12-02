using Helpers;
namespace BO;

/// <summary>
/// Represents an order that is currently in progress, including details about delivery and customer information.
/// </summary>
/// <remarks>This class provides information about an ongoing order, such as delivery and order identifiers,
/// customer details, and timing information related to the delivery process. It is used to track the status and
/// progress of an order from initiation to completion.</remarks>
public class OrderInProgress
{ 
    int DeliveryId { get; init; }
    int OrderId { get; init; }
    OrderType TypeOfOrder { get; init; }
    string? Description { get; init; }
    string Address { get; init; }
    double AirDistance { get; init; }
    double? ActualDistance { get; init; }
    string CustomerName { get; init; }
    string CustomerPhone { get; init; }
    DateTime OrderOpeningTime { get; init; }
    DateTime DeliveryStartTime { get; init; }
    DateTime ExpectedDeliveryTime { get; init; }
    DateTime MaxDeliveryTime { get; init; }
    OrderEndStatus StatusOfOrder { get; init; }
    SchedualeStatus TimeLinessStatus { get; init; }
    TimeSpan RemainingDeliveryTime { get; init; }
    public override string ToString() => this.ToStringProperty(); // Uses Helpers.ToStringProperty extension method
}
