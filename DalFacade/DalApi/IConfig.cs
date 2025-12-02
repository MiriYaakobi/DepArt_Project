namespace DalApi;

/// <summary>
/// Represents a configuration interface for managing time-related settings and operations.
/// </summary>
/// <remarks>This interface provides properties to configure various time ranges and a method to reset the
/// configuration.</remarks>
public interface IConfig
{
    DateTime Clock { get; set; } // Current system clock time //
    TimeSpan MaxDeliveryRange { get; set; } // Maximum allowed delivery time range //
    TimeSpan RiskRange { get; set; } // Time range considered as high risk //
    TimeSpan InactivityTimeRange { get; set; } // Time range for inactivity monitoring //
    string? CompenyAddress { get; set; }
    double? CompenyLatitude { get; set; }
    double? CompenyLongitude { get; set; }
    int AdminId { get; set; }
    double? DeliveryMaxDistance { get; set; }
    double AverageVehicleSpeedKmH { get; set; }
    double AverageMotorcycleSpeedKmH { get; set; }
    double AverageBicycleSpeedKmH { get; set; }
    double AverageByFootSpeedKmH { get; set; }
    void Reset();
}
