using System.Runtime.CompilerServices;
using DalApi;

namespace Helpers;

/// <summary>
/// Internal BL manager for all Application's Configuration Variables and Clock logic policies
/// </summary>
internal static class AdminManager
{
    #region Stage 4-7
    private static readonly IDal s_dal = Factory.Get;

    /// <summary>
    /// Property for providing current application's clock value for any BL class that may need it
    /// </summary>
    internal static DateTime Now { get => s_dal.Config.Clock; }

    internal static event Action? ConfigUpdatedObservers;
    internal static event Action? ClockUpdatedObservers;

    // Background tasks for simulation logic
    private static Task? _periodicTask = null;
    private static Task? _simulationTask = null; // Stage 7

    /// <summary>
    /// Method to update application's clock from any BL class as may be required.
    /// Triggers background maintenance and simulation tasks.
    /// </summary>
    /// <param name="newClock">updated clock value</param>
    internal static void UpdateClock(DateTime newClock)
    {
        var oldClock = s_dal.Config.Clock;
        s_dal.Config.Clock = newClock;

        // Periodic Updates (Cleanup inactive couriers)
        if (_periodicTask is null || _periodicTask.IsCompleted)
            _periodicTask = Task.Run(() => CourierManager.PeriodicCourierUpdates(oldClock, newClock));

        // Simulation Logic (Couriers picking orders, delivering, etc.) - Stage 7
        if (_simulationTask is null || _simulationTask.IsCompleted)
            _simulationTask = Task.Run(() => CourierManager.SimulateCourierActivityAsync());

        // Calling all the observers of clock update
        ClockUpdatedObservers?.Invoke();
    }

    /// <summary>
    /// Retrieves the current configuration settings for the application.
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
    internal static BO.Config GetConfig()
    => new BO.Config()
    {
        AdminId = s_dal.Config.AdminId,
        DeliveryMaxDistance = s_dal.Config.DeliveryMaxDistance,
        Clock = s_dal.Config.Clock,
        MaxDeliveryRange = s_dal.Config.MaxDeliveryRange,
        RiskRange = s_dal.Config.RiskRange,
        InactivityTimeRange = s_dal.Config.InactivityTimeRange,
        CompenyAddress = s_dal.Config.CompenyAddress,
        CompenyLatitude = s_dal.Config.CompenyLatitude,
        CompenyLongitude = s_dal.Config.CompenyLongitude,
        AverageVehicleSpeedKmH = s_dal.Config.AverageVehicleSpeedKmH,
        AverageMotorcycleSpeedKmH = s_dal.Config.AverageMotorcycleSpeedKmH,
        AverageBicycleSpeedKmH = s_dal.Config.AverageBicycleSpeedKmH,
        AverageByFootSpeedKmH = s_dal.Config.AverageByFootSpeedKmH
    };

    /// <summary>
    /// updates the application's configuration settings after validating the provided data.
    /// </summary>
    /// <param name="configuration"></param>
    internal static void SetConfig(BO.Config configuration)
    {
        ThrowOnSimulatorIsRunning();

        // Validation logic
        if (configuration.AdminId <= 100000000 || configuration.AdminId > 999999999)
            throw new BO.BlInvalidDataException("ID must be a 9-digit number");

        // Company address validation
        if (string.IsNullOrWhiteSpace(configuration.CompenyAddress))
            throw new BO.BlInvalidDataException("Company address cannot be empty.");

        // Check positive distances
        if (configuration.DeliveryMaxDistance <= 0)
            throw new BO.BlInvalidDataException("Max delivery distance must be positive.");

        // Check positive speeds
        if (configuration.AverageVehicleSpeedKmH <= 0 ||
            configuration.AverageMotorcycleSpeedKmH <= 0 ||
            configuration.AverageBicycleSpeedKmH <= 0 ||
            configuration.AverageByFootSpeedKmH <= 0)
        {
            throw new BO.BlInvalidDataException("All speed values must be positive.");
        }

        // Password validation
        if (!string.IsNullOrEmpty(configuration.AdminPassword) && configuration.AdminPassword.Length < 8)
            throw new BO.BlInvalidDataException("Password must be at least 8 characters long.");

        bool configChanged = false;

        // Update address if changed
        if (s_dal.Config.CompenyAddress != configuration.CompenyAddress)
        {
            if (configuration.CompenyAddress.Length < 3)
                throw new BO.BlInvalidDataException("Address is too short to be valid.");

            // Verify address and get coordinates
            var coordinates = Tools.GetCoordinatesOfAddressSync(configuration.CompenyAddress);
            if (coordinates == null)
                throw new BO.BlDoesNotExistException($"Address verification failed: '{configuration.CompenyAddress}' was not found.");

            s_dal.Config.CompenyAddress = configuration.CompenyAddress;
            s_dal.Config.CompenyLatitude = coordinates.Value.Latitude;
            s_dal.Config.CompenyLongitude = coordinates.Value.Longitude;
            configChanged = true;
        }

        // Helper local function to reduce code duplication
        void UpdateIfChanged<T>(T newValue, T currentValue, Action<T> updateAction)
        {
            if (!EqualityComparer<T>.Default.Equals(newValue, currentValue))
            {
                updateAction(newValue);
                configChanged = true;
            }
        }

        // Update other properties
        UpdateIfChanged(configuration.AdminId, s_dal.Config.AdminId, val => s_dal.Config.AdminId = val);
        UpdateIfChanged(configuration.MaxDeliveryRange, s_dal.Config.MaxDeliveryRange, val => s_dal.Config.MaxDeliveryRange = val);
        UpdateIfChanged(configuration.RiskRange, s_dal.Config.RiskRange, val => s_dal.Config.RiskRange = val);
        UpdateIfChanged(configuration.InactivityTimeRange, s_dal.Config.InactivityTimeRange, val => s_dal.Config.InactivityTimeRange = val);
        UpdateIfChanged(configuration.DeliveryMaxDistance, s_dal.Config.DeliveryMaxDistance, val => s_dal.Config.DeliveryMaxDistance = val);
        UpdateIfChanged(configuration.AverageVehicleSpeedKmH, s_dal.Config.AverageVehicleSpeedKmH, val => s_dal.Config.AverageVehicleSpeedKmH = val);
        UpdateIfChanged(configuration.AverageMotorcycleSpeedKmH, s_dal.Config.AverageMotorcycleSpeedKmH, val => s_dal.Config.AverageMotorcycleSpeedKmH = val);
        UpdateIfChanged(configuration.AverageBicycleSpeedKmH, s_dal.Config.AverageBicycleSpeedKmH, val => s_dal.Config.AverageBicycleSpeedKmH = val);
        UpdateIfChanged(configuration.AverageByFootSpeedKmH, s_dal.Config.AverageByFootSpeedKmH, val => s_dal.Config.AverageByFootSpeedKmH = val);

        // Update password if provided
        if (!string.IsNullOrEmpty(configuration.AdminPassword))
        {
            UpdateIfChanged(configuration.AdminPassword, s_dal.Config.AdminPassword, val => s_dal.Config.AdminPassword = val);
        }

        // Notify observers if configuration changed
        if (configChanged)
            ConfigUpdatedObservers?.Invoke();
    }

    /// <summary>
    /// Resets the database to its initial state and updates the system configuration.
    /// </summary>
    internal static void ResetDB()
    {
        // Ensure simulator is not running
        ThrowOnSimulatorIsRunning();

        // Perform reset within a lock to ensure thread safety
        lock (BlMutex)
        {
            s_dal.ResetDB();
            AdminManager.UpdateClock(DateTime.Now);
            AdminManager.SetConfig(AdminManager.GetConfig());
        }
    }

    /// <summary>
    /// Initializes the database and updates the system configuration.
    /// </summary>
    internal static void InitializeDB()
    {
        // Ensure simulator is not running
        ThrowOnSimulatorIsRunning();

        // Perform initialization within a lock to ensure thread safety
        lock (BlMutex)
        {
            DalTest.Initialization.Do();
            AdminManager.UpdateClock(DateTime.Now);
            AdminManager.SetConfig(AdminManager.GetConfig());
        }
    }

    /// <summary>
    /// checks whether the requesting user is an admin
    /// </summary>
    public static void AssertAdmin(int requestingUserId)
    {
        // Check if the requesting user is not the admin
        if (requestingUserId != s_dal.Config.AdminId)
            throw new BO.BlNotAuthorizedException($"User ID {requestingUserId} is not authorized to perform this administrative action.");
    }

    /// <summary>
    /// checks whether the requesting user is an admin or the target user itself
    /// </summary>
    public static void AssertAdminOrSelf(int requestingUserId, int targetId)
    {
        // Check if the requesting user is neither the admin nor the target user
        if (requestingUserId != s_dal.Config.AdminId && requestingUserId != targetId)
            throw new BO.BlNotAuthorizedException($"User ID {requestingUserId} is not authorized to access data for courier {targetId}.");
    }

    #endregion Stage 4-7

    #region Stage 7 base

    /// <summary>    
    /// Mutex to use from BL methods to get mutual exclusion while the simulator is running
    /// </summary>
    internal static readonly object BlMutex = new();

    /// <summary>
    /// The thread of the simulator
    /// </summary>
    private static volatile Thread? s_thread;

    /// <summary>
    /// The Interval for clock updating in minutes by second
    /// </summary>
    private static int s_interval = 1;

    /// <summary>
    /// The flag that signs whether simulator is running
    /// </summary>
    private static volatile bool s_stop = false;

    [MethodImpl(MethodImplOptions.Synchronized)]
    public static void ThrowOnSimulatorIsRunning()
    {
        // Throw exception if simulator is running
        if (s_thread is not null)
            throw new BO.BlTemporaryNotAvailableException("Cannot perform the operation since Simulator is running");
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    internal static void Start(int interval)
    {
        // Start the simulator thread if not already running
        if (s_thread is null)
        {
            s_interval = interval;
            s_stop = false;
            s_thread = new(clockRunner) { Name = "ClockRunner" };
            s_thread.Start();
        }
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    internal static void Stop()
    {
        // Stop the simulator thread if running
        if (s_thread is not null)
        {
            s_stop = true;
            s_thread.Interrupt(); //awake a sleeping thread
            s_thread.Name = "ClockRunner stopped";
            s_thread = null;
        }
    }

    /// <summary>
    /// clock runner method for the simulator thread
    /// </summary>
    private static void clockRunner()
    {
        // Main loop for the simulator thread
        while (!s_stop)
        {
            DateTime newTime = Now.AddMinutes(s_interval);
            UpdateClock(newTime);

            Task.Run(async () =>
            {
                // create new orders periodically
                await OrderManager.SimulateNewOrderCreationAsync();

                // couriers simulate their activity
                await CourierManager.SimulateCourierActivityAsync();

                // close orders periodically
                await OrderManager.PeriodicOrderUpdatesAsync(newTime);
            });

            try { Thread.Sleep(1000); } catch { }
        }
    }

    #endregion Stage 7 base
}