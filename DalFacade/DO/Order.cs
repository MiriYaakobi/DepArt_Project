namespace DO;

public record Order
(
  int Id, //Entity ID number//
  OrderType TypeOfOrder,
  string Address,
  double Latitude,
  double Longitude,
  string CustomerName,
  string CustomerPhone,
  DateTime OrderOpeningTime,
  string? PackageDetails = null,
  string? Description = null
)
{
    public Order() : this(0, OrderType.Regular, " ", 0.0, 0.0, " ", " ", DateTime.MinValue, null, null) { }
}