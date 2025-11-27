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
    int Id { get; init; }
    OrderType TypeOfOrder { get; set; }
    string? Description { get; set; }
    string Address { get; set; }
    double Latitude { get; }
    double Longitude { get; }
    double AirDistance { get; }
    string CustomerName { get; set; }
    string CustomerPhone { get; set; }
    string? PackageDetails { get; set; }
    DateTime OrderOpeningTime { get; init; }
    DateTime? ExpectedDeliveryTime { get; init; }
    DateTime MaxDeliveryTime { get; init; }
    OrderEndStatus StatusOfOrder { get; init; }
    SchedualeStatus TimeLinessStatus { get; init; }
    TimeSpan RemainingDeliveryTime { get; init; }
    DeliveryPerOrderInList DeliveryList { get; init; }
    public override string ToString() => this.ToStringProperty(); // Uses Helpers.ToStringProperty extension method
}
