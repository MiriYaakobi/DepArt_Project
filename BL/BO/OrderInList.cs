namespace BO;

public class OrderInList
{
    int? Id { get; init; }
    int OrderId { get; init; }
    OrderType TypeOfOrder { get; init; }
    double AirDistance { get; init; }
    OrderStatus StatusOfOrder { get; init; }
    SchedualeStatus TimeLinessStatus { get; init; }
    TimeSpan RemainingTime { get; init; }
    TimeSpan TotalHandlingDuration { get; init; }
    int TotalDeliveries { get; init; }
}
