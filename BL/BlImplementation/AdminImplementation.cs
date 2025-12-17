namespace BlImplementation;
using BLApi;
using Helpers;

internal class AdminImplementation : IAdmin
{
    public void ForwardClock(BO.TimeUnit unit)
    {
        DateTime newClock = unit switch
        {
            BO.TimeUnit.Minutes => AdminManager.Now.AddMinutes(1),
            BO.TimeUnit.Hours => AdminManager.Now.AddHours(1),
            BO.TimeUnit.Days => AdminManager.Now.AddDays(1),
            BO.TimeUnit.Months => AdminManager.Now.AddMonths(1),
            BO.TimeUnit.Years => AdminManager.Now.AddYears(1),

            _ => throw new BO.BlInvalidDataException($"Unsupported time unit for clock forward: {unit}")
        };

        //update the clock using the AdminManager
        AdminManager.UpdateClock(newClock);
    }

    public DateTime GetClock() => AdminManager.Now;

    public BO.Config GetConfig() => AdminManager.GetConfig();

    public void InitializeDB()
    {
        AdminManager.InitializeDB();
    }

    public void ResetDB()
    {
        AdminManager.ResetDB();
    }

    public void SetConfig(BO.Config config)
    {
        AdminManager.SetConfig(config);
    }
}
