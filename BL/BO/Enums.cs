namespace BO;

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

public enum OrderEndStatus
{
    Delivered,
    Refused,
    Cancelled,
    Failed,
    InviterNotFound
}

public enum OrderStatus
{
    Open,
    InProgress,
    Delivered,
    Refused,
    Cancelled,
}

public enum SchedualeStatus
{
    OnTime,
    InRisk,
    Late
}