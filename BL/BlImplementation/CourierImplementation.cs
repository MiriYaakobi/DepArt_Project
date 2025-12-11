namespace BlImplementation;
using BLApi;
using Helpers;

internal class CourierImplementation : ICourier
{
    public void Create(int requestingUserId, BO.Courier boCourier)
    {

        Login(requestingUserId);

        try
        {
            CourierManager.CreateCourier(boCourier);
        }
        catch (ArgumentException ex)
        {
            throw new BO.BlInvalidDataException(ex.Message, ex);
        }
        catch (InvalidOperationException ex)
        {
            throw new BO.BlAlreadyExistsException(ex.Message, ex);
        }
    }

    public void Delete(int requestingUserId, int courierId)
    {
        CourierManager.DeleteCourier(courierId);
    }

    public BO.UserRole Login(int userId, string password)
    {
        try
        {
            return CourierManager.Login(userId, password);
        }
        catch (ArgumentException ex)
        {
            throw new BO.BlLoginFailedException(ex.Message);
        }
    }

    public BO.Courier? Read(int requestingUserId, int courierId)
    {
        return CourierManager.ReadCourier(courierId);
    }

    public IEnumerable<BO.CourierInList> ReadAll(int requestingUserId, bool? isActive = null, BO.CourierFieldSort? sortBy = null)
    {

    }

    public void Update(int requestingUserId, BO.Courier boCourier)
    {
        CourierManager.UpdateCourier(boCourier);
    }

//-------------------------------------------------------------------------------------------
private static void RequireAdmin(int requestingUserId)
    {
        var role = CourierManager.Login(requestingUserId, "__BYPASS__");

        if (role != BO.UserRole.Admin)
            throw new BO.BlNotAuthorizedException("Only administrators may perform this action.");
    }

    private static void RequireAdminOrSelf(int requestingUserId, int courierId)
    {
        BO.UserRole role;

        try
        {
            role = CourierManager.Login(requestingUserId, "__BYPASS__");
        }
        catch
        {
            throw new BO.BlNotAuthorizedException("Unauthorized access.");
        }

        if (role == BO.UserRole.Admin)
            return;

        if (role == BO.UserRole.Courier && requestingUserId == courierId)
            return;

        throw new BO.BlNotAuthorizedException("You do not have permission to perform this action.");
    }
}