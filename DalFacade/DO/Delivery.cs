namespace DO;
public record Delivery
(
    int Id, //Entity ID number//
    int OrderId,
    int CourierId,
    OrderType TypeOfOrder,
    DateTime DeliveryStartTime,
    double? ActualDistance = null,
    OrderStatus? OrderEndStatus = null,
    DateTime? DeliveryEndTime = null
)
{
    public Delivery() : this(0, 0, 0, OrderType.Regular, DateTime.MinValue, null, null, null) { }
}