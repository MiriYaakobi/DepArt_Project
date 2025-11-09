namespace DO;

/// <summary>
/// Represents a delivery record with details about the order, courier, and delivery status.
/// </summary>
/// <remarks>This record is used to track the delivery process, including the start and end times, the type of
/// order, and the status upon completion.</remarks>
/// <param name="Id">The unique identifier for the delivery entity.</param>
/// <param name="OrderId">The identifier of the order associated with this delivery.</param>
/// <param name="CourierId">The identifier of the courier responsible for the delivery.</param>
/// <param name="TypeOfOrder">The type of order being delivered, indicating whether it is regular or another type.</param>
/// <param name="DeliveryStartTime">The date and time when the delivery process started.</param>
/// <param name="ActualDistance">The actual distance covered during the delivery, in kilometers. This value is optional and may be null if not
/// applicable.</param>
/// <param name="OrderEndStatus">The status of the order at the end of the delivery. This value is optional and may be null if the delivery is not
/// yet completed.</param>
/// <param name="DeliveryEndTime">The date and time when the delivery process ended. This value is optional and may be null if the delivery is not yet
/// completed.</param>
public record Delivery
(
    int Id, //Entity ID number//
    int OrderId,
    int CourierId,
    OrderType TypeOfOrder, //Regular or other//
    DateTime DeliveryStartTime, //When the delivery started//
    double? ActualDistance = null,
    OrderStatus? OrderEndStatus = null, //Status when delivery ended//
    DateTime? DeliveryEndTime = null //When the delivery ended//
)
{
    public Delivery() : this(0, 0, 0, OrderType.Regular, DateTime.MinValue, null, null, null) { } //Default constructor//
}