namespace BO;

/// <summary>
/// Specifies the mode of delivery for a package.
/// </summary>
/// <remarks>This enumeration is used to indicate the type of vehicle or method used for delivering packages. It
/// can be used to determine the appropriate delivery route or calculate delivery times based on the mode of
/// transport.</remarks>
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
/// Represents the final status of an order in the system.
/// </summary>
/// <remarks>This enumeration is used to indicate the outcome of an order process.  Each value corresponds to a
/// specific end state that an order can reach.</remarks>
public enum OrderEndStatus
{
    Delivered,
    Refused,
    Cancelled,
    Failed,
    InviterNotFound
}

/// <summary>
/// Represents the various states an order can be in during its lifecycle.
/// </summary>
/// <remarks>This enumeration is used to track the current status of an order.  It helps in determining the
/// progress and handling of orders within the system.</remarks>
public enum OrderStatus
{
    Open,
    InProgress,
    Delivered,
    Refused,
    Cancelled,
}

/// <summary>
/// Represents the status of a schedule in terms of its adherence to planned timelines.
/// </summary>
/// <remarks>This enumeration is used to indicate whether a schedule is proceeding as planned, at risk of delay,
/// or already late.</remarks>
public enum ScheduleStatus
{
    OnTime,
    InRisk,
    Late
}

/// <summary>
/// תפקידי המשתמשים במערכת לצורך אימות כניסה (פנימי ל-BL).
/// </summary>
public enum UserRole
{
    Courier,
    Admin
}