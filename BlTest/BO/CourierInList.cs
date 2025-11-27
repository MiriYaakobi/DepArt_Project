namespace BO;

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
}
