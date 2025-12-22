namespace BlApi;

/// <summary>
/// Provides administrative operations for managing the database and system configuration.
/// </summary>
/// <remarks>This interface defines methods for resetting and initializing the database, manipulating the system
/// clock, and getting or setting configuration settings. It is intended for use by system administrators or services
/// with elevated privileges.</remarks>
public interface IAdmin
{
    void ResetDB();
    void InitializeDB();
    DateTime GetClock();
    void ForwardClock(BO.TimeUnit unit);
    BO.Config GetConfig();
    void SetConfig(BO.Config config);
}