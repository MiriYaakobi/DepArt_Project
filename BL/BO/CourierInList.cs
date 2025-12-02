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
    int Id { get; init; }
    string Name { get; init; }
    bool IsActive { get; init; }
    DeliveryType TypeOfDelivery { get; init; }
    DateTime StartWorkTime { get; init; }
    int TotalOnTimeDeliveries { get; init; }
    int TotalLateDeliveries { get; init; }
    int? CurrentOrderId { get; init; }
    public override string ToString() => this.ToStringProperty(); // Uses Helpers.ToStringProperty extension method
}
