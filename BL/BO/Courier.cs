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
    public int Id { get; init; }
    public string? Name { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public bool IsActive { get; set; }
    public double? MaxDistance { get; set; }
    public DeliveryType TypeOfDelivery { get; set; }
    public DateTime StartWorkTime { get; init; }
    public int TotalOnTimeDeliveries { get; init; }
    public int TotalLateDeliveries { get; init; }
    public OrderInProgress? CurrentOrder { get; init; } //OrderInProgress
    public override string ToString() => this.ToStringProperty(); // Uses Helpers.ToStringProperty extension method
}
