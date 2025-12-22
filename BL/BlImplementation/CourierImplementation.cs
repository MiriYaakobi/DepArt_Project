namespace BlImplementation;
using BlApi;
using Helpers;

/// <summary>
/// a courier implementation of the business logic layer
/// </summary>
/// <remarks>
/// encapsulates all courier-related business logic operations
/// </remarks>
internal class CourierImplementation : ICourier
{
    /// <summary>
    /// creates a new courier in the system
    /// </summary>
    /// <param name="requestingUserId"></param>
    /// <param name="boCourier"></param>
    /// <exception cref="BO.BlAlreadyExistsException"></exception>
    public void Create(int requestingUserId, BO.Courier boCourier)
    {
        //access control: only admin can create couriers
        AdminManager.AssertAdmin(requestingUserId);

        //validate courier data
        CourierManager.ValidateCourierData(boCourier);

        try
        {
            //create the courier
            CourierManager.CreateCourier(boCourier);
        }

        catch (DO.DalAlreadyExistsException ex)
        {
            throw new BO.BlAlreadyExistsException($"Courier with ID {boCourier.Id} already exists.", ex);
        }
    }

    /// <summary>
    /// deletes an existing courier from the system
    /// </summary>
    /// <param name="requestingUserId"></param>
    /// <param name="courierId"></param>
    /// <exception cref="BO.BlCannotDeleteException"></exception>
    /// <exception cref="BO.BlDoesNotExistException"></exception>
    public void Delete(int requestingUserId, int courierId)
    {
        //access control: only admin can delete couriers
        AdminManager.AssertAdmin(requestingUserId);

        // check if the courier has handled any orders
        if (CourierManager.IsCourierUsed(courierId))
            throw new BO.BlCannotDeleteException($"Cannot delete courier {courierId} because they have handled or are currently handling orders.");

        try
        {
            //delete the courier
            CourierManager.DeleteCourier(courierId);
        }

        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException($"Courier with ID {courierId} does not exist.", ex);
        }
    }

    /// <summary>
    /// logins a courier using their ID and password
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    /// <exception cref="BO.BlLoginFailedException"></exception>
    public BO.UserRole Login(int userId, string password)
    {
        //input validation
        if (userId <= 0 || string.IsNullOrEmpty(password))
        {
            throw new BO.BlLoginFailedException($"User ID or password cannot be empty.");
        }

        // attempt to login
        BO.UserRole role = CourierManager.Login(userId, password);

        return role;
    }

    /// <summary>
    /// gets the details of a specific courier
    /// </summary>
    /// <param name="requestingUserId"></param>
    /// <param name="courierId"></param>
    /// <returns></returns>
    /// <exception cref="BO.BlDoesNotExistException"></exception>
    public BO.Courier? Read(int requestingUserId, int courierId)
    {
        //access control: only admin or the courier themselves can read the details
        AdminManager.AssertAdminOrSelf(requestingUserId, courierId);

        //get the courier details
        BO.Courier? boCourier = CourierManager.ReadCourier(courierId);

        //handle case where courier does not exist
        if (boCourier == null)
            throw new BO.BlDoesNotExistException($"Courier with ID {courierId} does not exist.");

        return boCourier;
    }

    /// <summary>
    /// gets a list of couriers with optional filtering and sorting.
    /// </summary>
    /// <param name="requestingUserId"></param>
    /// <param name="isActive"></param>
    /// <param name="sortBy"></param>
    /// <returns></returns>
    public IEnumerable<BO.CourierInList> ReadAll(int requestingUserId, bool? isActive = null, BO.CourierFieldSort? sortBy = null)
    {
        //access control: only admin can read the list of couriers
        AdminManager.AssertAdmin(requestingUserId);

        //get the list of couriers with optional filtering
        IEnumerable<BO.CourierInList> couriers = CourierManager.ReadAllCouriers(isActive);

        //sort the list if a sort field is provided
        if (sortBy.HasValue)
            couriers = CourierManager.SortCouriersBy(couriers, sortBy.Value);
        else
            couriers = couriers.OrderBy(c => c.Id);

        return couriers;
    }

    /// <summary>
    /// updates the details of an existing courier
    /// </summary>
    /// <param name="requestingUserId"></param>
    /// <param name="boCourier"></param>
    /// <exception cref="BO.BlDoesNotExistException"></exception>
    public void Update(int requestingUserId, BO.Courier boCourier)
    {
        //access control: only admin or the courier themselves can update the details
        AdminManager.AssertAdminOrSelf(requestingUserId, boCourier.Id);

        //validate courier data
        CourierManager.ValidateCourierData(boCourier);

        //validate courier data
        try
        {
            CourierManager.UpdateCourier(boCourier);
        }

        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException($"Courier with ID {boCourier.Id} does not exist.", ex);
        }
    }

    public void AddObserver(Action listObserver) =>
        CourierManager.Observers.AddListObserver(listObserver);
    public void AddObserver(int id, Action observer) =>
        CourierManager.Observers.AddObserver(id, observer);
    public void RemoveObserver(Action listObserver) =>
        CourierManager.Observers.RemoveListObserver(listObserver);
    public void RemoveObserver(int id, Action observer) =>
        CourierManager.Observers.RemoveObserver(id, observer);
}