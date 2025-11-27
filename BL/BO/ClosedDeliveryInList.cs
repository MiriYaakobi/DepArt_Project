namespace BO;

public class ClosedDeliveryInList
{
    int Id { get; init; }
    int OrderId { get; init; }
    OrderType TypeOfOrder { get; init; }
    string Address { get; init; }
    DeliveryType TypeOfDelivery { get; init; }
    double? ActualDistance { get; init; }
    TimeSpan TotalHandlingDuration { get; init; }
    OrderEndStatus OrderClosedStatus { get; init; }
}
