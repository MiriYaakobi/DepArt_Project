namespace BO;

/// <summary>
/// Represents the configuration settings for the delivery system.
/// </summary>
/// <remarks>This class provides properties to configure various aspects of the delivery system,  including time
/// ranges for delivery and inactivity, company location details,  and average speeds for different modes of
/// transportation.</remarks>
public class Config
{
    public DateTime Clock { get; set; } // Current system clock time
    public TimeSpan MaxDeliveryRange { get; set; } // Maximum allowed delivery time range
    public TimeSpan RiskRange { get; set; } // Time range considered as high risk
    public TimeSpan InactivityTimeRange { get; set; } // Time range for inactivity monitoring
    public string? CompenyAddress { get; set; }
    public double? CompenyLatitude { get; set; }
    public double? CompenyLongitude { get; set; }
    public int AdminId { get; set; }
    public double? DeliveryMaxDistance { get; set; }
    public double AverageVehicleSpeedKmH { get; set; }
    public double AverageMotorcycleSpeedKmH { get; set; }
    public double AverageBicycleSpeedKmH { get; set; }
    public double AverageByFootSpeedKmH { get; set; }
    //
    int NextOrderId { get; set; }
    int NextDeliveryId { get; set; }
}