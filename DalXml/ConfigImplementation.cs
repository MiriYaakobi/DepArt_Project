namespace Dal;
using DalApi;

/// <summary>
/// a class that implements the IConfig interface to manage configuration settings.
/// </summary>
internal class ConfigImplementation : IConfig
{

    //properties
    public DateTime Clock
    {
        get => Config.Clock;
        set => Config.Clock = value;
    }
    public TimeSpan MaxDeliveryRange
    {
        get => Config.MaxDeliveryRange;
        set => Config.MaxDeliveryRange = value;
    }
    public TimeSpan RiskRange
    {
        get => Config.RiskRange;
        set => Config.RiskRange = value;
    }
    public TimeSpan InactivityTimeRange
    {
        get => Config.InactivityTimeRange;
        set => Config.InactivityTimeRange = value;
    }

    public string? CompenyAddress
    {
        get => Config.CompenyAddress;
        set => Config.CompenyAddress = value;
    }
    public double? CompenyLatitude
    {
        get => Config.CompenyLatitude;
        set => Config.CompenyLatitude = value;
    }
    public double? CompenyLongitude
    {
        get => Config.CompenyLongitude;
        set => Config.CompenyLongitude = value;
    }

    //reset method
    public void Reset()
    {
        Config.Reset();
    }

    //We added for step 11, will we use later?
    public int AdminId
    {
        get => Config.AdminId;
        set => Config.AdminId = value;
    }
    public double? DeliveryMaxDistance
    {
        get => Config.DeliveryMaxDistance;
        set => Config.DeliveryMaxDistance = value;
    }
    public double AverageVehicleSpeedKmH
    {
        get => Config.AverageVehicleSpeedKmH;
        set => Config.AverageVehicleSpeedKmH = value;
    }
    public double AverageMotorcycleSpeedKmH
    {
        get => Config.AverageMotorcycleSpeedKmH;
        set => Config.AverageMotorcycleSpeedKmH = value;
    }
    public double AverageBicycleSpeedKmH
    {
        get => Config.AverageBicycleSpeedKmH;
        set => Config.AverageBicycleSpeedKmH = value;
    }
    public double AverageByFootSpeedKmH
    {
        get => Config.AverageByFootSpeedKmH;
        set => Config.AverageByFootSpeedKmH = value;
    }
}