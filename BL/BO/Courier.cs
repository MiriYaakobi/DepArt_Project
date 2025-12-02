using Helpers;
namespace BO;

/// <summary>
/// Represents a courier responsible for delivering orders.
/// </summary>
/// <remarks>The <see cref="Courier"/> class encapsulates details about a courier, including personal information,
/// delivery statistics, and current order status. It provides properties to manage and access the courier's
/// information, such as contact details and delivery performance metrics.</remarks>
public class Courier
{
    int Id { get; init; }
    string Name { get; set; }
    string Phone { get; set; }
    string Email { get; set; }
    string Password { get; set; }
    bool IsActive { get; set; }
    double? MaxDistance { get; set; }
    DeliveryType TypeOfDekivery { get; set; }
    DateTime StartWorkTime { get; init; }
    int TotalOnTimeDeliveries { get; init; }
    int TotalLateDeliveries { get; init; }
    OrderInProgress? CurrentOrder { get; init; } //?
    public override string ToString() => this.ToStringProperty(); // Uses Helpers.ToStringProperty extension method
}
