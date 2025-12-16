namespace DalApi;

/// <summary>
/// Represents a configuration interface for managing time-related settings and operations.
/// </summary>
/// <remarks>This interface provides properties to configure various time ranges and a method to reset the
/// configuration.</remarks>
public interface IConfig
{
    int AdminId { get; set; }
    string AdminPassword { get; set; }
    DateTime Clock { get; set; }  
    TimeSpan MaxDeliveryRange { get; set; } 
    TimeSpan RiskRange { get; set; } 
    TimeSpan InactivityTimeRange { get; set; } 
    string? CompenyAddress { get; set; } 
    double? CompenyLatitude { get; set; }
    double? CompenyLongitude { get; set; }
    double? DeliveryMaxDistance { get; set; }
    double AverageVehicleSpeedKmH { get; set; }
    double AverageMotorcycleSpeedKmH { get; set; }
    double AverageBicycleSpeedKmH { get; set; }
    double AverageByFootSpeedKmH { get; set; }
    void Reset();
}
