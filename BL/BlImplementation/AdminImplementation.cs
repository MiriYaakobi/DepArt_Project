using BlApi;
using Helpers;

namespace BlImplementation;

/// <summary>
/// admin implementation of the BL API
/// </summary>
/// <remarks>
/// implements the IAdmin interface
/// </remarks>
internal class AdminImplementation : IAdmin
{
    /// <summary>
    /// forward the simulated clock by a given time unit
    /// </summary>
    /// <param name="unit"></param>
    /// <exception cref="BO.BlInvalidDataException"></exception>
    public void ForwardClock(BO.TimeUnit unit)
    {
        //calculate the new clock time based on the given time unit
        DateTime newClock = unit switch
        {
            BO.TimeUnit.Minutes => AdminManager.Now.AddMinutes(1),
            BO.TimeUnit.Hours => AdminManager.Now.AddHours(1),
            BO.TimeUnit.Days => AdminManager.Now.AddDays(1),
            BO.TimeUnit.Months => AdminManager.Now.AddMonths(1),
            BO.TimeUnit.Years => AdminManager.Now.AddYears(1),

            //default
            _ => throw new BO.BlInvalidDataException($"Unsupported time unit for clock forward: {unit}")
        };

        //update the clock using the AdminManager
        AdminManager.UpdateClock(newClock);
    }

    public DateTime GetClock() =>
        AdminManager.Now;

    public BO.Config GetConfig() =>
        AdminManager.GetConfig();

    public void InitializeDB() =>
        AdminManager.InitializeDB();

    public void ResetDB() =>
        AdminManager.ResetDB();

    public void SetConfig(BO.Config config) =>
        AdminManager.SetConfig(config);

    public void AddClockObserver(Action clockObserver) =>
        AdminManager.ClockUpdatedObservers += clockObserver;

    public void RemoveClockObserver(Action clockObserver) =>
        AdminManager.ClockUpdatedObservers -= clockObserver;

    public void AddConfigObserver(Action configObserver) =>
        AdminManager.ConfigUpdatedObservers += configObserver;

    public void RemoveConfigObserver(Action configObserver) =>
        AdminManager.ConfigUpdatedObservers -= configObserver;

}
