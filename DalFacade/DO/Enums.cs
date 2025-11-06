namespace DO;

internal class Enums
{

}
/// <summary>
/// Specifies the available types of delivery methods.
/// </summary>
/// <remarks>This enumeration is used to indicate the mode of transportation for deliveries.</remarks>
public enum DeliveryType
{
    Car,
    Motorcycle,
    Bicycle,
    ByFoot
}
/// <summary>
/// Specifies the type of order based on delivery speed.
/// </summary>
/// <remarks>This enumeration is used to indicate the delivery preference for an order.</remarks>
public enum OrderType
{
    Regular,
    Express,
    SameDay
}
/// <summary>
/// Represents the various statuses that an order can have during its lifecycle.
/// </summary>
/// <remarks>This enumeration is used to indicate the current state of an order, which can be one of the
/// following: <list type="bullet"> <item> <description><see cref="Delivered"/>: The order has been successfully
/// delivered to the recipient.</description> </item> <item> <description><see cref="Refused"/>: The order was refused
/// by the recipient upon delivery.</description> </item> <item> <description><see cref="Cancelled"/>: The order was
/// cancelled before it could be delivered.</description> </item> <item> <description><see cref="Failed"/>: The order
/// could not be delivered due to a failure in the process.</description> </item> <item> <description><see
/// cref="InviterNotFound"/>: The order could not be processed because the inviter was not found.</description> </item>
/// </list></remarks>
public enum OrderStatus
{
    Delivered,
    Refused,
    Cancelled,
    Failed,
    InviterNotFound
}