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
    void Reset();
}