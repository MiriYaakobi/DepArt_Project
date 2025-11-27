namespace BO;

public class DeliveryPerOrderInList
{
    int Id { get; init; }
    int? CourierId { get; init; }
    string Name { get; init; }
    DeliveryType TypeOfDelivery { get; init; }
    DateTime DeliveryStartTime { get; init; }
    OrderEndStatus? OrderClosedStatus { get; init; }
    DateTime? DeliveryEndTime { get; init; }
}
