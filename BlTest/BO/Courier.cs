namespace BO;

public class Courier
{
    int Id { get; init; }
    string Name { get; set; }
    string Phone { get; set; }
    string Email { get; set; }
    string Password { get; set; }
    bool IsActive { get; set; }
    double? MaxDistance { get; set; }
    DeliveryType TypeOfDekivery { get; set; }
    DateTime StartWorkTime { get; init; }
    int TotalOnTimeDeliveries { get; init; }
    int TotalLateDeliveries { get; init; }
    OrderInProgress? CurrentOrder { get; init; } //?
}
