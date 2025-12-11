using DalApi;

namespace Helpers;

internal static class DeliveryManager
{
    private static IDal s_dal = Factory.Get;

    /// <summary>
    /// Reads a single delivery record (internal).
    /// </summary>
    internal static DO.Delivery ReadDelivery(int deliveryId)
    {
        // Validate existence by calling the helper method.
        return GetExistingDelivery(deliveryId);
    }

    /// <summary>
    /// Reads all delivery records, optionally filtered by a predicate (internal).
    /// </summary>
    internal static IEnumerable<DO.Delivery> ReadAllDeliveries(Func<DO.Delivery, bool>? predicate = null)
    {
        return s_dal.Delivery.ReadAll(predicate);
    }

    /// <summary>
    /// TO_DO: מתודה ליצירת רשומת Delivery חדשה כחלק מ-OrderManager.ChooseOrder.
    /// הלוגיקה תהיה מורכבת: בדיקת תקינות שליח, עדכון סטטוס שליח, והפעלת s_dal.Delivery.Create.
    /// </summary>
    internal static void CreateNewDeliveryForOrder(int orderId, int courierId)
    {
        // הלוגיקה הזו תמומש במלואה בפרק 9 (מימוש OrderImplementation).
        throw new NotImplementedException();
    }

    /// <summary>
    /// TO_DO: מתודה לעדכון רשומת Delivery קיימת כחלק מ-OrderManager.CompleteDelivery.
    /// הלוגיקה תהיה מורכבת: חישוב מרחק בפועל, עדכון זמנים, עדכון OrderClosedStatus, ועדכון סטטוס שליח.
    /// </summary>
    internal static void CompleteDeliveryUpdate(int deliveryId, DO.OrderEndStatus completionStatus)
    {
        // הלוגיקה הזו תמומש במלואה בפרק 9 (מימוש OrderImplementation).
        throw new NotImplementedException();
    }

    /// <summary>
    /// Helper method to get an existing delivery or throw an exception if it does not exist.
    /// </summary>
    /// <param name="deliveryId"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private static DO.Delivery GetExistingDelivery(int deliveryId)
    {
        try
        {
            return s_dal.Delivery.Read(deliveryId)!;
        }
        catch (DO.DalDoesNotExistException)
        {
            throw new InvalidOperationException($"Delivery with ID {deliveryId} does not exist.");
        }
    }
}

