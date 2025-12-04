using DalApi;

namespace Helpers;

internal static class OrderManager
{
    private static IDal s_dal = Factory.Get;

    /// <summary>
    /// מבצעת בדיקות תקינות, ממירה כתובת לקואורדינטות, ויוצרת הזמנה חדשה ב-DAL.
    /// </summary>
    /// <param name="order">ישות BO.Order ליצירה.</param>
    /// <exception cref="ArgumentException">אם נתוני ההזמנה אינם תקינים (כולל כתובת לא נמצאה).</exception>
    internal static void CreateOrder(BO.Order order)
    {
        // 1. בדיקות תקינות קלט
        if (string.IsNullOrEmpty(order.CustomerName) || order.CustomerName.Length < 2)
            throw new ArgumentException("Customer name must contain at least 2 characters.");
        // TO_DO: בדיקת תקינות נוספת לטלפון (CustomerPhone).

        if (string.IsNullOrWhiteSpace(order.Address))
            throw new ArgumentException("Delivery address cannot be empty.");

        // 2. Geocoding: המרת כתובת לקואורדינטות
        var coordinates = Tools.GetCoordinatesOfAddressSync(order.Address);
        if (coordinates == null)
        {
            // אם כתובת הלקוח לא תקינה, זורקים חריגה
            throw new ArgumentException($"Address '{order.Address}' is invalid or could not be found.");
        }

        // 3. מיפוי (Mapping) ושמירה ב-DAL
        DO.Order doOrder = new DO.Order
        (
            Id: default, // ה-ID האמיתי יגיע מ-Dal.Config.NextOrderId
            TypeOfOrder: (DO.OrderType)order.TypeOfOrder,
            Address: order.Address,
            Latitude: coordinates.Value.Latitude, // הקואורדינטות שהתקבלו
            Longitude: coordinates.Value.Longitude,
            CustomerName: order.CustomerName!,
            CustomerPhone: order.CustomerPhone!,
            OrderOpeningTime: AdminManager.Now, // זמן פתיחה הוא שעון המערכת
            PackageDetails: order.PackageDetails,
            Description: order.Description
        );

        s_dal.Order.Create(doOrder);
    }

    /// <summary>
    /// קוראת רשומת הזמנה מה-DAL, ממירה אותה ל-BO, ומשלבת סטטוסים לוגיים ומשלוחים.
    /// </summary>
    /// <param name="orderId">מזהה ההזמנה.</param>
    /// <returns>ישות BO.Order מלאה.</returns>
    /// <exception cref="InvalidOperationException">אם ההזמנה לא נמצאת.</exception>
    internal static BO.Order ReadOrder(int orderId)
    {
        DO.Order doOrder;
        try
        {
            doOrder = s_dal.Order.Read(orderId);
        }
        catch (DO.DalDoesNotExistException)
        {
            throw new InvalidOperationException($"Order with ID {orderId} does not exist.");
        }

        // 2. חישוב סטטוסים לוגיים (נדרשים לפני המיפוי)
        BO.OrderStatus statusOfOrder = CalculateOrderStatus(orderId);
        BO.ScheduleStatus timeLinessStatus = CalculateScheduleStatus(orderId);
        DateTime maxDeliveryTime = CalculateMaxDeliveryTime(orderId);

        // 3. **השלמת חישוב AirDistance (השלמת TO_DO)**
        // AirDistance הוא המרחק בין כתובת החברה לכתובת ההזמנה.
        double airDistance = Tools.GetAirDistance(
            s_dal.Config.CompenyLatitude ?? 0,
            s_dal.Config.CompenyLongitude ?? 0,
            doOrder.Latitude,
            doOrder.Longitude
        );

        // 4. אחזור פרטי משלוחים (DeliveryPerOrderInList) - נשאר TO_DO להמשך
        BO.DeliveryPerOrderInList? deliveryList = null;

        // 5. מיפוי (Mapping) ל-BO (פתרון שגיאות CS0200)
        return new BO.Order
        {
            // שדות BO.Order
            Id = doOrder.Id,
            TypeOfOrder = (BO.OrderType)doOrder.TypeOfOrder,
            Description = doOrder.Description,
            Address = doOrder.Address,

            Latitude = doOrder.Latitude,
            Longitude = doOrder.Longitude,
            AirDistance = airDistance, // **השלמת AirDistance**

            CustomerName = doOrder.CustomerName,
            CustomerPhone = doOrder.CustomerPhone,
            PackageDetails = doOrder.PackageDetails,
            OrderOpeningTime = doOrder.OrderOpeningTime,

            // TO_DO: חישוב ExpectedDeliveryTime (דורש לוגיקה נוספת)
            ExpectedDeliveryTime = null,

            MaxDeliveryTime = maxDeliveryTime,
            StatusOfOrder = statusOfOrder,
            TimeLinessStatus = timeLinessStatus,

            // TO_DO: חישוב RemainingDeliveryTime (דורש לוגיקה נוספת)
            RemainingDeliveryTime = maxDeliveryTime - AdminManager.Now, // חישוב מידי

            DeliveryList = deliveryList
        };
    }

    /// <summary>
    /// קוראת את רשימת ההזמנות המלאה מ-DAL וממירה אותם לישויות רשימה (BO.OrderInList).
    /// </summary>
    /// <returns>IEnumerable של BO.OrderInList.</returns>
    internal static IEnumerable<BO.OrderInList> ReadAllOrders()
    {
        // 1. קריאה מ-DAL
        var dalOrders = s_dal.Order.ReadAll();

        // 2. מיפוי לישויות BO.OrderInList
        return dalOrders.Select(doOrder =>
        {
            int orderId = doOrder.Id;

            // חישוב סטטוסים
            BO.OrderStatus statusOfOrder = CalculateOrderStatus(orderId);
            BO.ScheduleStatus timeLinessStatus = CalculateScheduleStatus(orderId);

            // TO_DO: חישוב נתונים מצטברים (TotalHandlingDuration, TotalDeliveries)
            TimeSpan totalHandlingDuration = TimeSpan.Zero;
            int totalDeliveries = 0;
            TimeSpan remainingTime = TimeSpan.Zero;

            // TO_DO: חישוב AirDistance 
            double airDistance = 0;

            return new BO.OrderInList
            {
                OrderId = orderId,
                TypeOfOrder = (BO.OrderType)doOrder.TypeOfOrder,
                AirDistance = airDistance,
                StatusOfOrder = statusOfOrder,
                TimeLinessStatus = timeLinessStatus,
                RemainingTime = remainingTime,
                TotalHandlingDuration = totalHandlingDuration,
                TotalDeliveries = totalDeliveries
            };
        });
        // TO_DO: יש לבצע מיון וסינון מלא בממשק הציבורי.
    }

    // הוספה ל OrderManager.cs:

    /// <summary>
    /// מבצעת בדיקות תקינות ומעדכנת פרטים ניתנים לשינוי של הזמנה קיימת.
    /// </summary>
    /// <param name="order">ישות BO.Order עם הערכים המעודכנים.</param>
    /// <exception cref="InvalidOperationException">אם ההזמנה לא נמצאה או סופקה/בוטלה.</exception>
    internal static void UpdateOrder(BO.Order order)
    {
        // 1. בדיקת קיום
        DO.Order existingOrder;
        try
        {
            existingOrder = s_dal.Order.Read(order.Id);
        }
        catch (DO.DalDoesNotExistException)
        {
            throw new InvalidOperationException($"Order with ID {order.Id} was not found for update.");
        }

        // 2. בדיקה לוגית: אסור לעדכן הזמנה אם היא כבר סופקה, סורבה או בוטלה
        BO.OrderStatus currentStatus = CalculateOrderStatus(order.Id);
        if (currentStatus == BO.OrderStatus.Delivered || currentStatus == BO.OrderStatus.Refused || currentStatus == BO.OrderStatus.Cancelled)
        {
            throw new InvalidOperationException($"Cannot update Order {order.Id}. Status is {currentStatus}.");
        }

        // 3. עדכון ה-DO באמצעות with (רק השדות הרלוונטיים)
        DO.Order updatedOrder = existingOrder with
        {
            TypeOfOrder = (DO.OrderType)order.TypeOfOrder,
            Description = order.Description,
            PackageDetails = order.PackageDetails,
            CustomerName = order.CustomerName!,
            CustomerPhone = order.CustomerPhone!
            // שימו לב: Address, Latitude, Longitude, OrderOpeningTime אינם ניתנים לעדכון כאן
        };

        // 4. קריאה ל-DAL לעדכון הרשומה
        s_dal.Order.Update(updatedOrder);
    }

    // הוספה ל OrderManager.cs:

    /// <summary>
    /// מוחקת הזמנה מהמערכת, רק אם אינה קשורה לאף רשומת משלוח.
    /// </summary>
    /// <param name="orderId">מזהה ההזמנה למחיקה.</param>
    /// <exception cref="InvalidOperationException">אם ההזמנה לא נמצאה או קשורה למשלוחים.</exception>
    internal static void DeleteOrder(int orderId)
    {
        // 1. בדיקת תלות: אסור למחוק אם קיימת רשומת Delivery כלשהי (פתוחה או סגורה)
        if (s_dal.Delivery.ReadAll(d => d.OrderId == orderId).Any())
        {
            throw new InvalidOperationException($"Cannot delete Order {orderId}: order is associated with existing deliveries (history).");
        }

        // 2. קריאה ל-DAL למחיקה 
        try
        {
            s_dal.Order.Delete(orderId);
        }
        catch (DO.DalDoesNotExistException)
        {
            throw new InvalidOperationException($"Order with ID {orderId} does not exist and cannot be deleted.");
        }
    }

    /// <summary>
    /// מחשב את זמן האספקה המירבי המותר להזמנה נתונה.
    /// **מתודה זו פותרת את שגיאת הקומפילציה ב-CourierManager.**
    /// </summary>
    /// <param name="orderId">מזהה ההזמנה.</param>
    /// <returns>DateTime המייצג את זמן הסיום המקסימלי.</returns>
    /// <exception cref="InvalidOperationException">אם ההזמנה לא נמצאת.</exception>
    internal static DateTime CalculateMaxDeliveryTime(int orderId)
    {
        DO.Order doOrder;
        try
        {
            // 1. קבלת ההזמנה
            doOrder = s_dal.Order.Read(orderId);
        }
        catch (DO.DalDoesNotExistException)
        {
            // תרגום חריגה של DAL לחריגה לוגית של BL
            throw new InvalidOperationException($"Order with ID={orderId} does not exist.");
        }

        // 2. קבלת טווח זמן האספקה המירבי מהתצורה
        TimeSpan maxTimeSpan = s_dal.Config.MaxDeliveryRange;

        // 3. חישוב זמן אספקה מירבי (זמן הפתיחה + טווח מירבי)
        return doOrder.OrderOpeningTime.Add(maxTimeSpan);
    }

    /// <summary>
    /// מתודת עזר: מחזירה את זמן סיום המשלוח המאוחר ביותר של הזמנה (אם סופקה או נסגרה).
    /// </summary>
    /// <param name="orderId">מזהה ההזמנה.</param>
    /// <returns>DateTime של סיום המשלוח המאוחר ביותר.</returns>
    /// <exception cref="InvalidOperationException">אם אין משלוחים סגורים להזמנה זו.</exception>
    internal static DateTime GetLastDeliveryEndTime(int orderId)
    {
        var closedDeliveries = s_dal.Delivery.ReadAll(d => d.OrderId == orderId && d.DeliveryEndTime.HasValue);

        if (!closedDeliveries.Any())
        {
            throw new InvalidOperationException($"Order ID={orderId} has no closed deliveries.");
        }

        return closedDeliveries.Max(d => d.DeliveryEndTime.Value);
    }

    /// <summary>
    /// מחשבת את סטטוס העמידה בזמנים (OnTime/InRisk/Late) של ההזמנה.
    /// </summary>
    /// <param name="orderId">מזהה ההזמנה.</param>
    /// <returns>BO.ScheduleStatus</returns>
    internal static BO.ScheduleStatus CalculateScheduleStatus(int orderId)
    {
        // קריאה ראשונה: לוודא שההזמנה קיימת
        s_dal.Order.Read(orderId);

        // מציאת משלוח פתוח (אם קיים)
        DO.Delivery? currentDelivery = s_dal.Delivery.ReadAll(d => d.OrderId == orderId && d.DeliveryEndTime == null).FirstOrDefault();

        DateTime maxDeliveryTime = CalculateMaxDeliveryTime(orderId);
        DateTime now = AdminManager.Now; // שימוש בשעון המערכת
        TimeSpan riskTimeSpan = s_dal.Config.RiskRange; // טווח הסיכון מהתצורה

        // 1. בדיקה אם ההזמנה סופקה
        if (currentDelivery == null && s_dal.Delivery.ReadAll(d => d.OrderId == orderId && d.DeliveryEndTime.HasValue).Any())
        {
            DateTime timeOfEnd = GetLastDeliveryEndTime(orderId);

            if (timeOfEnd > maxDeliveryTime)
            {
                return BO.ScheduleStatus.Late; // סופקה באיחור
            }
            return BO.ScheduleStatus.OnTime; // סופקה בזמן
        }
        else // 2. אם ההזמנה פתוחה או בטיפול
        {
            TimeSpan timeRemaining = maxDeliveryTime - now;

            if (timeRemaining.TotalSeconds <= 0)
            {
                return BO.ScheduleStatus.Late; // חרג מהזמן המירבי
            }
            if (timeRemaining <= riskTimeSpan)
            {
                return BO.ScheduleStatus.InRisk; // בתוך טווח הסיכון
            }
            return BO.ScheduleStatus.OnTime; // יש מספיק זמן
        }
    }

    /// <summary>
    /// מחשבת את סטטוס ההזמנה הכללי (Open, InProgress, Delivered, Refused, Cancelled) על פי מצב המשלוחים שלה.
    /// </summary>
    /// <param name="orderId">מזהה ההזמנה.</param>
    /// <returns>BO.OrderStatus</returns>
    internal static BO.OrderStatus CalculateOrderStatus(int orderId)
    {
        // קריאה ראשונה: לוודא שההזמנה קיימת
        s_dal.Order.Read(orderId);

        // 1. מציאת משלוח פתוח
        DO.Delivery? currentDelivery = s_dal.Delivery.ReadAll(d => d.OrderId == orderId && d.DeliveryEndTime == null).FirstOrDefault();
        if (currentDelivery != null)
        {
            return BO.OrderStatus.InProgress; // יש משלוח פתוח = בטיפול
        }

        // 2. מציאת המשלוח הסגור האחרון
        DO.Delivery? lastClosedDelivery = s_dal.Delivery.ReadAll(d => d.OrderId == orderId && d.DeliveryEndTime.HasValue)
                                            .OrderByDescending(d => d.DeliveryEndTime)
                                            .FirstOrDefault();

        // 3. קביעת הסטטוס על פי סוג סיום המשלוח האחרון
        if (lastClosedDelivery == null)
        {
            return BO.OrderStatus.Open; // אין משלוחים כלל = פתוחה
        }

        switch (lastClosedDelivery.OrderClosedStatus)
        {
            case DO.OrderEndStatus.Delivered:
                return BO.OrderStatus.Delivered;
            case DO.OrderEndStatus.Refused:
                return BO.OrderStatus.Refused;
            case DO.OrderEndStatus.Cancelled:
                return BO.OrderStatus.Cancelled;

            case DO.OrderEndStatus.InviterNotFound:
            case DO.OrderEndStatus.Failed:
                // אם הסיום היה כשל או מזמין לא נמצא, ההזמנה חוזרת לסטטוס 'פתוחה' לטיפול מחדש.
                return BO.OrderStatus.Open;
            default:
                return BO.OrderStatus.Open;
        }
    }
}
