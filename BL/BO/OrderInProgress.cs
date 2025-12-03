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
    public int DeliveryId { get; init; }
    public int OrderId { get; init; }
    public OrderType TypeOfOrder { get; init; }
    public string? Description { get; init; }
    public string? Address { get; init; }
    public double AirDistance { get; init; }
    public double? ActualDistance { get; init; }
    public string? CustomerName { get; init; }
    public string? CustomerPhone { get; init; }
    public DateTime OrderOpeningTime { get; init; }
    public DateTime DeliveryStartTime { get; init; }
    public DateTime ExpectedDeliveryTime { get; init; }
    public DateTime MaxDeliveryTime { get; init; }
    public OrderEndStatus StatusOfOrder { get; init; }
    public SchedualeStatus TimeLinessStatus { get; init; }
    public TimeSpan RemainingDeliveryTime { get; init; }
    public override string ToString() => this.ToStringProperty(); // Uses Helpers.ToStringProperty extension method
}
