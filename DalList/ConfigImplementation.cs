namespace Dal;
using DalApi;
public class ConfigImplementation : IConfig
{
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
    public void Reset()
    {
        Config.Reset();
    }

    //הוספנו בשביל שלב 11, האם להוריד בהמשך?
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