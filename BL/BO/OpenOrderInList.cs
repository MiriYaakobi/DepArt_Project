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
    int? Id { get; init; }
    int OrderId { get; init; }
    OrderType TypeOfOrder { get; init; }
    string PackageDetails { get; init; }
    string Address { get; init; }
    double AirDistance { get; init; }
    double? ActualDistance { get; init; }
    TimeSpan? ActualTimeExtension { get; init; }
    SchedualeStatus TimeLinessStatus { get; init; }
    TimeSpan RemainingTime { get; init; }
    DateTime MaxDeliveryTime { get; init; }
    public override string ToString() => this.ToStringProperty(); // Uses Helpers.ToStringProperty extension method
}
