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
/// Specifies the role of a user within the system.
/// </summary>
/// <remarks>This enumeration is used to define the different roles that a user can have,  such as <see
/// cref="Courier"/> or <see cref="Admin"/>. Each role may have  different permissions and access levels within the
/// application.</remarks>
public enum UserRole
{
    Courier,
    Admin,
    None
}

/// <summary>
/// Represents units of time for various operations.
/// </summary>
/// <remarks>This enumeration is used to specify time intervals in terms of minutes, hours, or days.</remarks>
public enum TimeUnit
{
    Minutes,
    Hours,
    Days,
    Months,
    Years
}

/// <summary>
/// Specifies the fields by which courier data can be sorted.
/// </summary>
/// <remarks>This enumeration is used to define the sorting criteria for courier-related operations. Each member
/// represents a specific field that can be used to order the results.</remarks>
public enum CourierFieldSort
{
    Id,
    Name,
    IsActive,
    TypeOfDelivery,
    StartWorkTime,
    TotalOnTimeDeliveries,
    TotalLateDeliveries,
    MaxDistance
}

/// <summary>
/// Specifies the fields by which order data can be sorted.
/// </summary>
/// <remarks>This enumeration is used to define the sorting criteria for order-related operations. Each member
/// represents a specific field that can be used to order the results.</remarks>
public enum OrderFieldSort
{
    Id,
    StatusOfOrder,
    TimeLinessStatus,
    OrderOpeningTime,
    MaxDeliveryTime,
    AirDistance
}

/// <summary>
/// Specifies the fields by which closed delivery data can be sorted.
/// </summary>
/// <remarks>This enumeration is used to define the sorting criteria for closed delivery-related operations. Each member
/// represents a specific field that can be used to order the results.</remarks>
public enum ClosedDeliveryFieldSort
{
    OrderId,
    CompletionType,
    TimeOfEnd,
    TotalHandlingTime
}
/// <summary>
/// 
/// </summary>
public enum OpenOrderFieldSort
{
    Id,
    TimeLinessStatus,
    ExpectedDeliveryTime, //Actual time estimate
    MaxDeliveryTime, //Total time remaining
    AirDistance, // sorting by air distance
    ActualDistance // sorting by actual distance
}