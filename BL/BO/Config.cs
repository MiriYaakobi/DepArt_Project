namespace BO;

/// <summary>
/// Represents the configuration settings for the delivery system.
/// </summary>
/// <remarks>This class provides properties to configure various aspects of the delivery system,  including time
/// ranges for delivery and inactivity, company location details,  and average speeds for different modes of
/// transportation.</remarks>
public class Config
{
    DateTime Clock { get; set; } // Current system clock time
    TimeSpan MaxDeliveryRange { get; set; } // Maximum allowed delivery time range
    TimeSpan RiskRange { get; set; } // Time range considered as high risk
    TimeSpan InactivityTimeRange { get; set; } // Time range for inactivity monitoring
    string? CompenyAddress { get; set; }
    double? CompenyLatitude { get; set; }
    double? CompenyLongitude { get; set; }
    int AdminId { get; set; }
    double? DeliveryMaxDistance { get; set; }
    double AverageVehicleSpeedKmH { get; set; }
    double AverageMotorcycleSpeedKmH { get; set; }
    double AverageBicycleSpeedKmH { get; set; }
    double AverageByFootSpeedKmH { get; set; }
}