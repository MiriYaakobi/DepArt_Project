namespace DO;

/// <summary>
/// Represents a courier responsible for delivering items.
/// </summary>
/// <remarks>This record holds information about a courier, including their contact details, delivery type, and
/// work schedule. It also indicates whether the courier is currently active.</remarks>
/// <param name="Id">The unique identifier for the courier.</param>
/// <param name="Name">The name of the courier.</param>
/// <param name="Phone">The phone number of the courier.</param>
/// <param name="Email">The email address of the courier.</param>
/// <param name="Password">The password for the courier's account. This should be stored securely.</param>
/// <param name="IsActive">Indicates whether the courier is currently active. <see langword="true"/> if active; otherwise, <see
/// langword="false"/>.</param>
/// <param name="TypeOfDelivery">The type of delivery the courier performs, such as by foot or vehicle.</param>
/// <param name="StartWorkTime">The time when the courier starts their workday.</param>
/// <param name="MaxDistance">The maximum distance, in kilometers, that the courier is willing to travel for deliveries. If <see
/// langword="null"/>, there is no set limit.</param>
public record Courier
(
    int Id, //Entity ID number//
    string Name,
    string Phone,
    string Email,
    string Password,
    bool IsActive, //is the courier active?//
    DeliveryType TypeOfDelivery,
    DateTime StartWorkTime,
    double? MaxDistance = null //in kilometers//
)
{
    public Courier() : this(0, " ", " ", " ", " ", true, DeliveryType.ByFoot, DateTime.MinValue, null) { } //Default constructor//

}