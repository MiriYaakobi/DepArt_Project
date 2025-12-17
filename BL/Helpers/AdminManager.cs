//using BO;
using System.Runtime.CompilerServices;

namespace Helpers;

/// <summary>
/// Internal BL manager for all Application's Configuration Variables and Clock logic policies
/// </summary>
internal static class AdminManager //stage 4
{
    #region Stage 4-7
    private static readonly DalApi.IDal s_dal = DalApi.Factory.Get; //stage 4
    
    /// <sum+mary>
    /// Property for providing current application's clock value for any BL class that may need it
    /// </summary>
    internal static DateTime Now { get => s_dal.Config.Clock; } //stage 4

    internal static event Action? ConfigUpdatedObservers; //stage 5 - for config update observers
    internal static event Action? ClockUpdatedObservers; //stage 5 - for clock update observers

    //private static Task? _periodicTask = null; //stage 7

    /// <summary>
    /// Method to update application's clock from any BL class as may be required
    /// </summary>
    /// <param name="newClock">updated clock value</param>
    internal static void UpdateClock(DateTime newClock) //stage 4-7
    {
        var oldClock = s_dal.Config.Clock; //stage 4
        s_dal.Config.Clock = newClock; //stage 4
        
        //Add calls here to any logic method that should be called periodically,
        //after each clock update
        //for example, Periodic students' updates:
        // - Go through all students to update properties that are affected by the clock update
        // - (students become not active after 5 years etc.)

        CourierManager.PeriodicCourierUpdates(oldClock, newClock); //stage 4. to be removed in stage 7 and replaced as below

        //TO_DO: //stage 7
        //if (_periodicTask is null || _periodicTask.IsCompleted) //stage 7
        //    _periodicTask = Task.Run(() => StudentManager.PeriodicStudentsUpdates(oldClock, newClock));
        //...

        //Calling all the observers of clock update
        ClockUpdatedObservers?.Invoke(); //prepared for stage 5
    }

    /// <summary>
    /// Method for providing current configuration variables values for any BL class that may need it
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    internal static BO.Config GetConfig() //stage 4
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
    /// Method for setting current configuration variables values for any BL class that may need it
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    internal static void SetConfig(BO.Config configuration) //stage 4
    {
        bool configChanged = false; // stage 5

        // update company address and its coordinates if changed
        if (s_dal.Config.CompenyAddress != configuration.CompenyAddress)
        {
            var coordinates = Tools.GetCoordinatesOfAddressSync(configuration.CompenyAddress ?? "");

            if (coordinates == null)
                throw new BO.BlDoesNotExistException($"Company address '{configuration.CompenyAddress}' is invalid or could not be found.");

            s_dal.Config.CompenyAddress = configuration.CompenyAddress;
            s_dal.Config.CompenyLatitude = coordinates.Value.Latitude;
            s_dal.Config.CompenyLongitude = coordinates.Value.Longitude;
            configChanged = true;
        }

        // update other configuration parameters if changed
        if (s_dal.Config.DeliveryMaxDistance != configuration.DeliveryMaxDistance)
        {
            s_dal.Config.DeliveryMaxDistance = configuration.DeliveryMaxDistance;
            configChanged = true;
        }
    
        if (s_dal.Config.MaxDeliveryRange != configuration.MaxDeliveryRange)
        {
            s_dal.Config.MaxDeliveryRange = configuration.MaxDeliveryRange;
            configChanged = true;
        }

        if (s_dal.Config.RiskRange != configuration.RiskRange)
        {
            s_dal.Config.RiskRange = configuration.RiskRange;
            configChanged = true;
        }

        if (s_dal.Config.InactivityTimeRange != configuration.InactivityTimeRange)
        {
            s_dal.Config.InactivityTimeRange = configuration.InactivityTimeRange;
            configChanged = true;
        }

        if (s_dal.Config.AdminId != configuration.AdminId)
        {
            s_dal.Config.AdminId = configuration.AdminId;
            configChanged = true;
        }

        if (s_dal.Config.DeliveryMaxDistance != configuration.DeliveryMaxDistance)
        {
            s_dal.Config.DeliveryMaxDistance = configuration.DeliveryMaxDistance;
            configChanged = true;
        }

        if (s_dal.Config.AverageVehicleSpeedKmH != configuration.AverageVehicleSpeedKmH)
        {
            s_dal.Config.AverageVehicleSpeedKmH = configuration.AverageVehicleSpeedKmH;
            configChanged = true;
        }

        if (s_dal.Config.AverageMotorcycleSpeedKmH != configuration.AverageMotorcycleSpeedKmH)
        {
            s_dal.Config.AverageMotorcycleSpeedKmH = configuration.AverageMotorcycleSpeedKmH;
            configChanged = true;
        }

        if (s_dal.Config.AverageBicycleSpeedKmH != configuration.AverageBicycleSpeedKmH)
        {
            s_dal.Config.AverageBicycleSpeedKmH = configuration.AverageBicycleSpeedKmH;
            configChanged = true;
        }

        if (s_dal.Config.AverageByFootSpeedKmH != configuration.AverageByFootSpeedKmH)
        {
            s_dal.Config.AverageByFootSpeedKmH = configuration.AverageByFootSpeedKmH;
            configChanged = true;
        }

        //Calling all the observers of configuration update
        if (configChanged) // stage 5
            ConfigUpdatedObservers?.Invoke(); // stage 5
    }

    internal static void ResetDB() //stage 4-7
    {
        lock (BlMutex) //stage 7
        {
            s_dal.ResetDB(); //stage 4
            AdminManager.UpdateClock(AdminManager.Now); //stage 5 - needed since we want the label on Pl to be updated
            AdminManager.SetConfig(AdminManager.GetConfig()); //stage 5 - needed to update PL 
        }
    }

    internal static void InitializeDB() //stage 4-7
    {
        lock (BlMutex) //stage 7
        {
            DalTest.Initialization.Do(); //stage 4
            AdminManager.UpdateClock(AdminManager.Now);  //stage 5 - needed since we want the label on Pl to be updated           
            AdminManager.SetConfig(AdminManager.GetConfig()); //stage 5 - needed for update the PL
        }
    }
    /// <summary>
    /// checks whether the requesting user is an admin
    /// </summary>
    /// <param name="requestingUserId"></param>
    /// <exception cref="BO.BlNotAuthorizedException"></exception>
    public static void AssertAdmin(int requestingUserId)
    {
        if (requestingUserId != s_dal.Config.AdminId)
        {
            throw new BO.BlNotAuthorizedException($"User ID {requestingUserId} is not authorized to perform this administrative action.");
        }
    }

    /// <summary>
    /// checks whether the requesting user is an admin or the target user itself
    /// </summary>
    /// <param name="requestingUserId"></param>
    /// <param name="targetId"></param>
    /// <exception cref="BO.BlNotAuthorizedException"></exception>
    public static void AssertAdminOrSelf(int requestingUserId, int targetId)
    {
        // Check if the requesting user is neither the admin nor the target user
        if (requestingUserId != s_dal.Config.AdminId && requestingUserId != targetId)
        {
            throw new BO.BlNotAuthorizedException($"User ID {requestingUserId} is not authorized to access data for courier {targetId}.");
        }
    }

    #endregion Stage 4-7

    #region Stage 7 base

    /// <summary>    
    /// Mutex to use from BL methods to get mutual exclusion while the simulator is running
    /// </summary>
    internal static readonly object BlMutex = new(); // BlMutex = s_dal; // This field is actually the same as s_dal - it is defined for readability of locks
    /// <summary>
    /// The thread of the simulator
    /// </summary>
    private static volatile Thread? s_thread;
    /// <summary>
    /// The Interval for clock updating
    /// in minutes by second (default value is 1, will be set on Start())    
    /// </summary>
    private static int s_interval = 1;
    /// <summary>
    /// The flag that signs whether simulator is running
    /// 
    private static volatile bool s_stop = false;

    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7                                                 
    public static void ThrowOnSimulatorIsRunning()
    {
        if (s_thread is not null)
            throw new BO.BlTemporaryNotAvailableException("Cannot perform the operation since Simulator is running");
    }

    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7                                                 
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

    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7                                                 
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

    private static Task? _simulateTask = null;

    private static void clockRunner()
    {
        while (!s_stop)
        {
            UpdateClock(Now.AddMinutes(s_interval));

            //TO_DO: //stage 7
            //Add calls here to any logic simulation that was required in stage 7
            //for example: course registration simulation
            if (_simulateTask is null || _simulateTask.IsCompleted)//stage 7
                //_simulateTask = Task.Run(() => StudentManager.SimulateCourseRegistrationAndGrade());

            //etc...

            try
            {
                Thread.Sleep(1000); // 1 second
            }
            catch (ThreadInterruptedException) { }
        }
    }

    #endregion Stage 7 base
}
