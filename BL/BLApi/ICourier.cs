namespace BlApi;

/// <summary>
/// Defines the operations available for managing courier entities within the system.
/// </summary>
/// <remarks>This interface provides methods for logging in, reading, updating, deleting, and creating courier
/// records. Implementations should ensure that appropriate authorization checks are performed based on the requesting
/// user's role.</remarks>
public interface ICourier : IObservable
{
    BO.UserRole Login(int userId, string password);
    void Create(int requestingUserId, BO.Courier boCourier);
    IEnumerable<BO.CourierInList> ReadAll(int requestingUserId, bool? isActive = null, BO.CourierFieldSort? sortBy = null);
    BO.Courier? Read(int requestingUserId, int courierId);
    void Update(int requestingUserId, BO.Courier boCourier);
    void Delete(int requestingUserId, int courierId);
}