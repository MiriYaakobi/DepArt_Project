namespace DalApi;
public interface IConfig
{
    DateTime Clock { get; set; }
    TimeSpan MaxDeliveryRange { get; set; }
    TimeSpan RiskRange { get; set; }
    TimeSpan InactivityTimeRange { get; set; }
    void Reset();
}
