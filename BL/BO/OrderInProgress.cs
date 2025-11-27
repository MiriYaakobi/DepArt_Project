namespace BO;

public class OrderInProgress
{ 
    int DeliveryId { get; init; }
    int OrderId { get; init; }
    OrderType TypeOfOrder { get; init; }
    string? Description { get; init; }
    string Address { get; init; }
    double AirDistance { get; init; }
    double? ActualDistance { get; init; }
    string CustomerName { get; init; }
    string CustomerPhone { get; init; }
    DateTime OrderOpeningTime { get; init; }
    DateTime DeliveryStartTime { get; init; }
    DateTime ExpectedDeliveryTime { get; init; }
    DateTime MaxDeliveryTime { get; init; }
    OrderEndStatus StatusOfOrder { get; init; }
    SchedualeStatus TimeLinessStatus { get; init; }
    TimeSpan RemainingDeliveryTime { get; init; }
}
 