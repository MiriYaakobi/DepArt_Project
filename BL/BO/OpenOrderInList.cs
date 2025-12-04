using Helpers;
namespace BO;

/// <summary>
/// Represents an open order in a list, including details such as order type, package information, and delivery metrics.
/// </summary>
/// <remarks>This class provides properties to access various details about an open order, such as its unique
/// identifiers,  delivery address, and timing information. It is designed to be immutable after
/// initialization.</remarks>
public class OpenOrderInList
{
    public int? Id { get; init; }
    public int OrderId { get; init; }
    public OrderType TypeOfOrder { get; init; }
    public string? PackageDetails { get; init; }
    public string? Address { get; init; }
    public double AirDistance { get; init; }
    public double? ActualDistance { get; init; }
    public TimeSpan? ActualTimeExtension { get; init; }
    public ScheduleStatus TimeLinessStatus { get; init; }
    public TimeSpan RemainingTime { get; init; }
    public DateTime MaxDeliveryTime { get; init; }
    public override string ToString() => this.ToStringProperty(); // Uses Helpers.ToStringProperty extension method
}
