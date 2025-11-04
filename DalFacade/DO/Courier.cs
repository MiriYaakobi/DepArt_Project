namespace DO;

public record Courier
(
    int Id, //Entity ID number//
    string Name,
    string Phone,
    string Email,
    string Password,
    bool IsActive,
    DeliveryType TypeOfDelivery,
    DateTime StartWorkTime,
    double? MaxDist = null
)
{
    public Courier() : this(0, " ", " ", " ", " ", true, DeliveryType.ByFoot, DateTime.MinValue, null) { }

}