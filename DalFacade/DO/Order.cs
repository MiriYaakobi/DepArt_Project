namespace DO;

/// <summary>
/// Represents a customer order with details such as location, customer information, and order type.
/// </summary>
/// <remarks>This record is used to encapsulate all relevant information about a customer's order, including
/// geolocation data and optional package details. It provides a default constructor for creating an order with default
/// values.</remarks>
/// <param name="Id"></param>
/// <param name="TypeOfOrder"></param>
/// <param name="Address"></param>
/// <param name="Latitude"></param>
/// <param name="Longitude"></param>
/// <param name="CustomerName"></param>
/// <param name="CustomerPhone"></param>
/// <param name="OrderOpeningTime"></param>
/// <param name="PackageDetails"></param>
/// <param name="Description"></param>
public record Order
(
  int Id, //Entity ID number//
  OrderType TypeOfOrder,
  string Address,
  double Latitude, //Geographical coordinates//
  double Longitude, //Geographical coordinates//
  string CustomerName,
  string CustomerPhone,
  DateTime OrderOpeningTime, // When the order was placed//
  string? PackageDetails = null, //Details about the package being delivered//
  string? Description = null //Additional details about the order//
)
{
    public Order() : this(0, OrderType.Regular, " ", 0.0, 0.0, " ", " ", DateTime.MinValue, null, null) { } //Default constructor//
}