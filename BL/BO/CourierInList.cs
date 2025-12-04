using Helpers;
namespace BO;

/// <summary>
/// Represents a courier in the list with details about their status and delivery performance.
/// </summary>
/// <remarks>This class provides information about a courier, including their identification, name, activity
/// status, type of delivery, work schedule, and delivery performance metrics. It is designed to be immutable after
/// initialization.</remarks>
public class CourierInList
{
    public int Id { get; init; }
    public string? Name { get; init; }
    public bool IsActive { get; init; }
    public DeliveryType TypeOfDelivery { get; init; }
    public DateTime StartWorkTime { get; init; }
    public int TotalOnTimeDeliveries { get; init; }
    public int TotalLateDeliveries { get; init; }
    public int? CurrentOrderId { get; init; }
    public override string ToString() => this.ToStringProperty(); // Uses Helpers.ToStringProperty extension method
}
