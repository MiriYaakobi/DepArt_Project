namespace DO;

internal class Enums
{

}
public enum DeliveryType
{
    Car,
    Motorcycle,
    Bicycle,
    ByFoot
}
public enum OrderType
{
    Regular,
    Express,
    SameDay
}
public enum OrderStatus
{
    Delivered,
    Refused,
    Cancelled,
    Failed,
    InviterNotFound
}