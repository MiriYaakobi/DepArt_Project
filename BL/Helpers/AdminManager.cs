//using System.Runtime.CompilerServices;

//namespace Helpers;

///// <summary>
///// Internal BL manager for all Application's Configuration Variables and Clock logic policies
///// </summary>
//internal static class AdminManager //stage 4
//{
//    #region Stage 4-7
//    private static readonly DalApi.IDal s_dal = DalApi.Factory.Get; //stage 4

//    /// <summary>
//    /// Property for providing current application's clock value for any BL class that may need it
//    /// </summary>
//    internal static DateTime Now { get => s_dal.Config.Clock; } //stage 4

//    internal static event Action? ConfigUpdatedObservers; //stage 5 - for config update observers
//    internal static event Action? ClockUpdatedObservers; //stage 5 - for clock update observers

//    private static Task? _periodicTask = null; //stage 7

//    /// <summary>
//    /// Method to update application's clock from any BL class as may be required
//    /// </summary>
//    /// <param name="newClock">updated clock value</param>
//    internal static void UpdateClock(DateTime newClock) //stage 4-7
//    {
//        var oldClock = s_dal.Config.Clock; //stage 4
//        s_dal.Config.Clock = newClock; //stage 4

//        //stage 7
//        if (_periodicTask is null || _periodicTask.IsCompleted) //stage 7
//            _periodicTask = Task.Run(() => CourierManager.PeriodicCourierUpdates(oldClock, newClock));

//        //Calling all the observers of clock update
//        ClockUpdatedObservers?.Invoke();
//    }

//    /// <summary>
//    /// Retrieves the current configuration settings for the application.
//    /// </summary>
//    /// <remarks>This method returns a new instance of the <see cref="BO.Config"/> class populated with
//    /// configuration values sourced from the underlying data access layer (DAL). The method is thread-safe due to the
//    /// use of the <see cref="MethodImplOptions.Synchronized"/> attribute.</remarks>
//    /// <returns>A <see cref="BO.Config"/> object containing the application's configuration settings, such as administrative
//    /// details, delivery parameters, company information, and average speed metrics.</returns>
//    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
//    internal static BO.Config GetConfig()
//    => new BO.Config()
//    {
//        AdminId = s_dal.Config.AdminId,
//        DeliveryMaxDistance = s_dal.Config.DeliveryMaxDistance,
//        Clock = s_dal.Config.Clock,
//        MaxDeliveryRange = s_dal.Config.MaxDeliveryRange,
//        RiskRange = s_dal.Config.RiskRange,
//        InactivityTimeRange = s_dal.Config.InactivityTimeRange,
//        CompenyAddress = s_dal.Config.CompenyAddress,
//        CompenyLatitude = s_dal.Config.CompenyLatitude,
//        CompenyLongitude = s_dal.Config.CompenyLongitude,
//        AverageVehicleSpeedKmH = s_dal.Config.AverageVehicleSpeedKmH,
//        AverageMotorcycleSpeedKmH = s_dal.Config.AverageMotorcycleSpeedKmH,
//        AverageBicycleSpeedKmH = s_dal.Config.AverageBicycleSpeedKmH,
//        AverageByFootSpeedKmH = s_dal.Config.AverageByFootSpeedKmH
//    };

//    /// <summary>
//    /// updates the application's configuration settings after validating the provided data.
//    /// </summary>
//    /// <param name="configuration"></param>
//    /// <exception cref="BO.BlInvalidDataException"></exception>
//    /// <exception cref="BO.BlDoesNotExistException"></exception>
//    internal static void SetConfig(BO.Config configuration)
//    {
//        //admin ID
//        if (configuration.AdminId <= 100000000 || configuration.AdminId > 999999999)
//            throw new BO.BlInvalidDataException("ID must be a 9-digit number");

//        if (string.IsNullOrWhiteSpace(configuration.CompenyAddress))
//            throw new BO.BlInvalidDataException("Company address cannot be empty.");

//        //check positive distances
//        if (configuration.DeliveryMaxDistance <= 0)
//            throw new BO.BlInvalidDataException("Max delivery distance must be positive.");

//        //check positive speeds
//        if (configuration.AverageVehicleSpeedKmH <= 0 ||
//            configuration.AverageMotorcycleSpeedKmH <= 0 ||
//            configuration.AverageBicycleSpeedKmH <= 0 ||
//            configuration.AverageByFootSpeedKmH <= 0)
//        {
//            throw new BO.BlInvalidDataException("All speed values must be positive.");
//        }

//        if (!string.IsNullOrEmpty(configuration.AdminPassword) && configuration.AdminPassword.Length < 8)
//            throw new BO.BlInvalidDataException("Password must be at least 8 characters long.");


//        //update progress flag
//        bool configChanged = false;

//        //update address if changed
//        if (s_dal.Config.CompenyAddress != configuration.CompenyAddress)
//        {
//            //logic checks for address
//            if (configuration.CompenyAddress.Length < 3)
//                throw new BO.BlInvalidDataException("Address is too short to be valid.");

//            var coordinates = Tools.GetCoordinatesOfAddressSync(configuration.CompenyAddress);
//            if (coordinates == null)
//                throw new BO.BlDoesNotExistException($"Address verification failed: '{configuration.CompenyAddress}' was not found.");

//            //update address and coordinates
//            s_dal.Config.CompenyAddress = configuration.CompenyAddress;
//            s_dal.Config.CompenyLatitude = coordinates.Value.Latitude;
//            s_dal.Config.CompenyLongitude = coordinates.Value.Longitude;
//            configChanged = true;
//        }

//        //helper local function to reduce code duplication
//        void UpdateIfChanged<T>(T newValue, T currentValue, Action<T> updateAction)
//        {
//            if (!EqualityComparer<T>.Default.Equals(newValue, currentValue))
//            {
//                updateAction(newValue);
//                configChanged = true;
//            }
//        }

//        //update other properties
//        UpdateIfChanged(configuration.AdminId, s_dal.Config.AdminId, val
//            => s_dal.Config.AdminId = val);

//        UpdateIfChanged(configuration.MaxDeliveryRange, s_dal.Config.MaxDeliveryRange, val
//            => s_dal.Config.MaxDeliveryRange = val);

//        UpdateIfChanged(configuration.RiskRange, s_dal.Config.RiskRange, val
//            => s_dal.Config.RiskRange = val);

//        UpdateIfChanged(configuration.InactivityTimeRange, s_dal.Config.InactivityTimeRange,
//            val => s_dal.Config.InactivityTimeRange = val);

//        UpdateIfChanged(configuration.DeliveryMaxDistance, s_dal.Config.DeliveryMaxDistance,
//            val => s_dal.Config.DeliveryMaxDistance = val);

//        UpdateIfChanged(configuration.AverageVehicleSpeedKmH, s_dal.Config.AverageVehicleSpeedKmH, val
//            => s_dal.Config.AverageVehicleSpeedKmH = val);

//        UpdateIfChanged(configuration.AverageMotorcycleSpeedKmH, s_dal.Config.AverageMotorcycleSpeedKmH, val
//            => s_dal.Config.AverageMotorcycleSpeedKmH = val);

//        UpdateIfChanged(configuration.AverageBicycleSpeedKmH, s_dal.Config.AverageBicycleSpeedKmH, val
//            => s_dal.Config.AverageBicycleSpeedKmH = val);

//        UpdateIfChanged(configuration.AverageByFootSpeedKmH, s_dal.Config.AverageByFootSpeedKmH, val
//            => s_dal.Config.AverageByFootSpeedKmH = val);

//        //update password if provided
//        if (!string.IsNullOrEmpty(configuration.AdminPassword))
//        {
//            UpdateIfChanged(configuration.AdminPassword, s_dal.Config.AdminPassword,
//                val => s_dal.Config.AdminPassword = val);
//        }

//        if (configChanged)
//            ConfigUpdatedObservers?.Invoke();
//    }

//    /// <summary>
//    /// Resets the database to its initial state and updates the system configuration.
//    /// </summary>
//    /// <remarks>This method performs a full reset of the database and ensures that the system clock  and
//    /// configuration are updated to reflect the current state. It is thread-safe and  should be used with caution as it
//    /// may result in data loss.</remarks>
//    internal static void ResetDB()
//    {
//        lock (BlMutex) //stage 7
//        {
//            s_dal.ResetDB();
//            AdminManager.UpdateClock(DateTime.Now);
//            AdminManager.SetConfig(AdminManager.GetConfig());
//        }
//    }

//    /// <summary>
//    /// Initializes the database and updates the system configuration.
//    /// </summary>
//    /// <remarks>This method performs the initial setup of the database by invoking the required
//    /// initialization routines. It also updates the system clock and applies the current configuration settings. The
//    /// method is thread-safe and ensures that only one initialization process occurs at a time.</remarks>
//    internal static void InitializeDB()
//    {
//        lock (BlMutex) //stage 7
//        {
//            DalTest.Initialization.Do();
//            AdminManager.UpdateClock(DateTime.Now);         
//            AdminManager.SetConfig(AdminManager.GetConfig());
//        }
//    }
//    /// <summary>
//    /// checks whether the requesting user is an admin
//    /// </summary>
//    /// <param name="requestingUserId"></param>
//    /// <exception cref="BO.BlNotAuthorizedException"></exception>
//    public static void AssertAdmin(int requestingUserId)
//    {
//        if (requestingUserId != s_dal.Config.AdminId)
//            throw new BO.BlNotAuthorizedException($"User ID {requestingUserId} is not authorized to perform this administrative action.");
//    }

//    /// <summary>
//    /// checks whether the requesting user is an admin or the target user itself
//    /// </summary>
//    /// <param name="requestingUserId"></param>
//    /// <param name="targetId"></param>
//    /// <exception cref="BO.BlNotAuthorizedException"></exception>
//    public static void AssertAdminOrSelf(int requestingUserId, int targetId)
//    {
//        // Check if the requesting user is neither the admin nor the target user
//        if (requestingUserId != s_dal.Config.AdminId && requestingUserId != targetId)
//            throw new BO.BlNotAuthorizedException($"User ID {requestingUserId} is not authorized to access data for courier {targetId}.");
//    }

//    #endregion Stage 4-7

//    #region Stage 7 base

//    /// <summary>    
//    /// Mutex to use from BL methods to get mutual exclusion while the simulator is running
//    /// </summary>
//    internal static readonly object BlMutex = new(); // BlMutex = s_dal; // This field is actually the same as s_dal - it is defined for readability of locks
//    /// <summary>
//    /// The thread of the simulator
//    /// </summary>
//    private static volatile Thread? s_thread;
//    /// <summary>
//    /// The Interval for clock updating
//    /// in minutes by second (default value is 1, will be set on Start())    
//    /// </summary>
//    private static int s_interval = 1;
//    /// <summary>
//    /// The flag that signs whether simulator is running
//    /// </summary>
//    private static volatile bool s_stop = false;

//    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7                                                 
//    public static void ThrowOnSimulatorIsRunning()
//    {
//        if (s_thread is not null)
//            throw new BO.BlTemporaryNotAvailableException("Cannot perform the operation since Simulator is running");
//    }

//    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7                                                 
//    internal static void Start(int interval)
//    {
//        if (s_thread is null)
//        {
//            s_interval = interval;
//            s_stop = false;
//            s_thread = new(clockRunner) { Name = "ClockRunner" };
//            s_thread.Start();
//        }
//    }

//    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7                                                 
//    internal static void Stop()
//    {
//        if (s_thread is not null)
//        {
//            s_stop = true;
//            s_thread.Interrupt(); //awake a sleeping thread
//            s_thread.Name = "ClockRunner stopped";
//            s_thread = null;
//        }
//    }

//    private static Task? _simulateTask = null;

//    private static void clockRunner()
//    {
//        while (!s_stop)
//        {
//            UpdateClock(Now.AddMinutes(s_interval));

//            //stage 7
//            //Add calls here to any logic simulation that was required in stage 7
//            //for example: course registration simulation
//            //if (_simulateTask is null || _simulateTask.IsCompleted)//stage 7
//            //    _simulateTask = Task.Run(() => StudentManager.SimulateCourseRegistrationAndGrade());

//            //etc...

//            try
//            {
//                Thread.Sleep(1000); // 1 second
//            }
//            catch (ThreadInterruptedException) { }
//        }
//    }

//    #endregion Stage 7 base
//}

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

        // 1. Periodic Updates (Cleanup inactive couriers)
        if (_periodicTask is null || _periodicTask.IsCompleted)
            _periodicTask = Task.Run(() => CourierManager.PeriodicCourierUpdates(oldClock, newClock));

        // 2. Simulation Logic (Couriers picking orders, delivering, etc.) - Stage 7
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
        // Validation logic
        if (configuration.AdminId <= 100000000 || configuration.AdminId > 999999999)
            throw new BO.BlInvalidDataException("ID must be a 9-digit number");

        if (string.IsNullOrWhiteSpace(configuration.CompenyAddress))
            throw new BO.BlInvalidDataException("Company address cannot be empty.");

        if (configuration.DeliveryMaxDistance <= 0)
            throw new BO.BlInvalidDataException("Max delivery distance must be positive.");

        if (configuration.AverageVehicleSpeedKmH <= 0 ||
            configuration.AverageMotorcycleSpeedKmH <= 0 ||
            configuration.AverageBicycleSpeedKmH <= 0 ||
            configuration.AverageByFootSpeedKmH <= 0)
        {
            throw new BO.BlInvalidDataException("All speed values must be positive.");
        }

        if (!string.IsNullOrEmpty(configuration.AdminPassword) && configuration.AdminPassword.Length < 8)
            throw new BO.BlInvalidDataException("Password must be at least 8 characters long.");

        bool configChanged = false;

        // Update address if changed
        if (s_dal.Config.CompenyAddress != configuration.CompenyAddress)
        {
            if (configuration.CompenyAddress.Length < 3)
                throw new BO.BlInvalidDataException("Address is too short to be valid.");

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

        if (!string.IsNullOrEmpty(configuration.AdminPassword))
        {
            UpdateIfChanged(configuration.AdminPassword, s_dal.Config.AdminPassword, val => s_dal.Config.AdminPassword = val);
        }

        if (configChanged)
            ConfigUpdatedObservers?.Invoke();
    }

    /// <summary>
    /// Resets the database to its initial state and updates the system configuration.
    /// </summary>
    internal static void ResetDB()
    {
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
        if (requestingUserId != s_dal.Config.AdminId)
            throw new BO.BlNotAuthorizedException($"User ID {requestingUserId} is not authorized to perform this administrative action.");
    }

    /// <summary>
    /// checks whether the requesting user is an admin or the target user itself
    /// </summary>
    public static void AssertAdminOrSelf(int requestingUserId, int targetId)
    {
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
        if (s_thread is not null)
            throw new BO.BlTemporaryNotAvailableException("Cannot perform the operation since Simulator is running");
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    internal static void Start(int interval)
    {
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
        if (s_thread is not null)
        {
            s_stop = true;
            s_thread.Interrupt(); //awake a sleeping thread
            s_thread.Name = "ClockRunner stopped";
            s_thread = null;
        }
    }

    private static void clockRunner()
    {
        while (!s_stop)
        {
            // Advance clock by 1 minute (simulation time)
            UpdateClock(Now.AddMinutes(1));

            try
            {
                // Sleep for s_interval seconds (real time)
                Thread.Sleep(s_interval * 1000);
            }
            catch (ThreadInterruptedException) { }
        }
    }

    #endregion Stage 7 base
}