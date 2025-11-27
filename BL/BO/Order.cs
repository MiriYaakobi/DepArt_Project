namespace BO;

internal class Order
{
    int Id { get; init; }
    OrderType TypeOfOrder { get; set; }
    string? Description { get; set; }
    string Address { get; set; }
    double Latitude { get; }
    double Longitude { get; }
    double AirDistance { get; }
    string CustomerName { get; set; }
    string CustomerPhone { get; set; }
    string? PackageDetails { get; set; }
    DateTime OrderOpeningTime { get; init; }
    DateTime? ExpectedDeliveryTime { get; init; }
    DateTime MaxDeliveryTime { get; init; }
    OrderEndStatus StatusOfOrder { get; init; }
    SchedualeStatus TimeLinessStatus { get; init; }
    TimeSpan RemainingDeliveryTime { get; init; }
    DeliveryPerOrderInList DeliveryList { get; init; } 
}
