using DalApi;

namespace Helpers;

internal static class CourierManager
{
    private static IDal s_dal = Factory.Get;
    internal static void AddCourier(BO.Courier courier)
    {
        if (string.IsNullOrEmpty(courier.Name) || courier.Name.Length < 2)
        {
            throw new Exception()
        }
    }
}
