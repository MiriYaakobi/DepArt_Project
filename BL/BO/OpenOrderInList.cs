namespace BO;

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
}
