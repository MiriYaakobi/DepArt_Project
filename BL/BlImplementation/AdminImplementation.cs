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
        AdminManager.ThrowOnSimulatorIsRunning();

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

    /// <summary>
    /// Validates the login credentials for the admin user.
    /// </summary>
    public bool ValidateLogin(int id, string password)
    {
        // Get the stored admin configuration
        BO.Config config = AdminManager.GetConfig();

        // Check if the provided ID matches the stored admin ID
        if (config.AdminId != id)
            return false;

        // Hash the input password and compare it with the stored hashed password
        string hashedInput = Helpers.Tools.HashPassword(password);

        return config.AdminPassword == hashedInput;
    }

    public DateTime GetClock() =>
        AdminManager.Now;

    public BO.Config GetConfig() =>
        AdminManager.GetConfig();

    public void InitializeDB()
    {
        AdminManager.ThrowOnSimulatorIsRunning();
        AdminManager.InitializeDB();
    }

    public void ResetDB()
    {
        AdminManager.ThrowOnSimulatorIsRunning();
        AdminManager.ResetDB();
    }

    public void SetConfig(BO.Config config)
    {
        AdminManager.ThrowOnSimulatorIsRunning();
        AdminManager.SetConfig(config);
    }

    public void AddClockObserver(Action clockObserver) =>
        AdminManager.ClockUpdatedObservers += clockObserver;

    public void RemoveClockObserver(Action clockObserver) =>
        AdminManager.ClockUpdatedObservers -= clockObserver;

    public void AddConfigObserver(Action configObserver) =>
        AdminManager.ConfigUpdatedObservers += configObserver;

    public void RemoveConfigObserver(Action configObserver) =>
        AdminManager.ConfigUpdatedObservers -= configObserver;

    /// <summary>
    /// Starts the simulator with the specified time interval.
    /// </summary>
    /// <param name="interval">Simulation speed (seconds per logical minute)</param>
    public void StartSimulator(int interval) 
    {
        // check if the simulator is already running
        AdminManager.ThrowOnSimulatorIsRunning();

        // Start the simulator
        AdminManager.Start(interval);
    }

    /// <summary>
    /// Stops the simulator.
    /// </summary>
    public void StopSimulator() => AdminManager.Stop(); 
}
