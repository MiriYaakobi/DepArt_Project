using Helpers;
namespace BO;

/// <summary>
/// Represents an order with details such as customer information, delivery address, and timing constraints.
/// </summary>
/// <remarks>This class encapsulates the essential information for processing and tracking an order, including
/// customer details, order type, and delivery specifics. It provides properties to access the order's geographical
/// location and timing constraints, which are crucial for logistics and scheduling.</remarks>
internal class Order
{
    public int Id { get; init; }
    public OrderType TypeOfOrder { get; set; }
    public string? Description { get; set; }
    public string? Address { get; set; }
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public double AirDistance { get; init; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public string? PackageDetails { get; set; }
    public DateTime OrderOpeningTime { get; init; }
    public DateTime? ExpectedDeliveryTime { get; init; }
    public DateTime MaxDeliveryTime { get; init; }
    public OrderStatus StatusOfOrder { get; init; }
    public ScheduleStatus TimeLinessStatus { get; init; }
    public TimeSpan RemainingDeliveryTime { get; init; }
    public DeliveryPerOrderInList? DeliveryList { get; init; }
    public override string ToString() => this.ToStringProperty(); // Uses Helpers.ToStringProperty extension method
}
