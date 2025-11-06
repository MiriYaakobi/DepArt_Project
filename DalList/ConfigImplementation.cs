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
    public void Reset()
    {
        Config.Reset();
    }
}