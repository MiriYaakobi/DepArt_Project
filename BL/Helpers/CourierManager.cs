using DalApi;

namespace Helpers;

internal static class CourierManager
{
    private static IDal s_dal = Factory.Get;

    /// <summary>
    /// מבצעת בדיקות תקינות לוגיות ומוסיפה שליח חדש ל-DAL.
    /// </summary>
    internal static void CreateCourier(BO.Courier courier)
    {
        // 1. בדיקות תקינות קלט (ArgumentException)
        if (string.IsNullOrEmpty(courier.Name) || courier.Name.Length < 2)
            throw new ArgumentException("Courier name must contain at least 2 characters.");
        if (string.IsNullOrEmpty(courier.Phone) || courier.Phone.Length != 10 || !courier.Phone.All(char.IsDigit))
            throw new ArgumentException("Phone number is invalid.");
        if (string.IsNullOrEmpty(courier.Password) || courier.Password.Length < 6)
            throw new ArgumentException("Password must be at least 6 characters long.");
        if (courier.MaxDistance.HasValue && courier.MaxDistance.Value <= 0)
            throw new ArgumentException("Max distance must be a positive value.");

        // 2. בדיקת כפילות ID (InvalidOperationException)
        try
        {
            s_dal.Courier.Read(courier.Id);
            throw new InvalidOperationException($"Courier with ID {courier.Id} already exists.");
        }
        catch (DO.DalDoesNotExistException)
        {
            // השליח לא קיים - תקין להמשיך
        }

        // 3. מיפוי (Mapping) ושמירה ב-DAL
        DO.Courier doCourier = new DO.Courier
        (
            Id: courier.Id,
            Name: courier.Name!,
            Phone: courier.Phone!,
            Email: courier.Email!,
            Password: courier.Password!,
            IsActive: true, // ברירת מחדל: שליח חדש הוא פעיל
            TypeOfDelivery: (DO.DeliveryType)courier.TypeOfDelivery,
            StartWorkTime: AdminManager.Now, // זמן התחלת עבודה הוא שעון המערכת
            MaxDistance: courier.MaxDistance
        );

        s_dal.Courier.Create(doCourier);
    }

    /// <summary>
    /// קוראת את רשימת השליחים המלאה מ-DAL וממירה אותם לישויות רשימה (BO.CourierInList).
    /// </summary>
    internal static IEnumerable<BO.CourierInList> ReadAllCouriers(bool? isActive = null)
    {
        // 1. קריאה מ-DAL וסינון לפי IsActive (אם נדרש)
        var dalCouriers = s_dal.Courier.ReadAll(isActive.HasValue ? d => d.IsActive == isActive.Value : null);

        // 2. מיפוי לישויות BO.CourierInList
        return dalCouriers.Select(doCourier =>
        {
            // חישוב סטטיסטיקות (דורש GetCourierStatistics)
            var stats = GetCourierStatistics(doCourier.Id);

            // TO_DO: יש לממש מתודת עזר שתמצא את מזהה ההזמנה הפתוחה לשליח (currentOrderId)
            int? currentOrderId = null; // דמה זמני

            return new BO.CourierInList
            {
                Id = doCourier.Id,
                Name = doCourier.Name,
                IsActive = doCourier.IsActive,
                TypeOfDelivery = (BO.DeliveryType)doCourier.TypeOfDelivery,
                StartWorkTime = doCourier.StartWorkTime,
                TotalOnTimeDeliveries = stats.DeliveredOnTime,
                TotalLateDeliveries = stats.DeliveredLate,
                CurrentOrderId = currentOrderId
            };
        });
        // TO_DO: המיון הלוגי (sort) ימומש בממשק ICourier.ReadAll, לאחר קבלת ה-IEnumerable.
    }

    /// <summary>
    /// קוראת רשומת שליח מה-DAL, ממירה אותה ל-BO, ומצרפת נתונים לוגיים וסטטיסטיים.
    /// </summary>
    internal static BO.Courier ReadCourier(int courierId)
    {
        DO.Courier doCourier;
        try
        {
            // 1. קריאה מ-DAL
            doCourier = s_dal.Courier.Read(courierId);
        }
        catch (DO.DalDoesNotExistException)
        {
            throw new InvalidOperationException($"Courier with ID {courierId} does not exist.");
        }

        // 2. חישוב סטטיסטיקות
        var stats = GetCourierStatistics(courierId);

        // 3. מציאת משלוח פתוח (CurrentOrder)
        // TO_DO: יש לממש את הלוגיקה שתמצא Delivery פתוח ותמיר אותו ל-BO.OrderInProgress.
        BO.OrderInProgress? currentOrder = null;

        // 4. מיפוי (Mapping) ל-BO
        return new BO.Courier
        {
            Id = doCourier.Id,
            Name = doCourier.Name,
            Phone = doCourier.Phone,
            Email = doCourier.Email,
            Password = doCourier.Password,
            IsActive = doCourier.IsActive,
            MaxDistance = doCourier.MaxDistance,
            TypeOfDelivery = (BO.DeliveryType)doCourier.TypeOfDelivery,
            StartWorkTime = doCourier.StartWorkTime,
            TotalOnTimeDeliveries = stats.DeliveredOnTime,
            TotalLateDeliveries = stats.DeliveredLate,
            CurrentOrder = currentOrder // יושלם בהמשך
        };
    }

    /// <summary>
    /// מבצעת בדיקות תקינות לוגיות ומעדכנת פרטים ניתנים לשינוי של שליח קיים.
    /// </summary>
    internal static void UpdateCourier(BO.Courier courier)
    {
        // 1. בדיקת קיום
        DO.Courier existingCourier;
        try
        {
            existingCourier = s_dal.Courier.Read(courier.Id);
        }
        catch (DO.DalDoesNotExistException)
        {
            throw new InvalidOperationException($"Courier with ID {courier.Id} was not found for update.");
        }

        // 2. בדיקות תקינות על הערכים החדשים (חייב להיות תקין לפני עדכון)
        if (string.IsNullOrEmpty(courier.Name) || courier.Name.Length < 2)
            throw new ArgumentException("Courier name must contain at least 2 characters.");
        // TO_DO: יש לבצע בדיקות על טלפון, סיסמה וכו' כפי שבוצעו ב-Create.

        // 3. עדכון ה-DO באמצעות with
        DO.Courier updatedCourier = existingCourier with
        {
            Name = courier.Name!,
            Phone = courier.Phone!,
            Email = courier.Email!,
            Password = courier.Password!,
            TypeOfDelivery = (DO.DeliveryType)courier.TypeOfDelivery,
            MaxDistance = courier.MaxDistance
        };

        // 4. קריאה ל-DAL לעדכון הרשומה
        s_dal.Courier.Update(updatedCourier);
    }

    /// <summary>
    /// מוחקת שליח מהמערכת, רק אם אינו קשור לאף משלוח (פתוח או סגור).
    /// </summary>
    internal static void DeleteCourier(int courierId)
    {
        // 1. בדיקת תלות: אסור למחוק אם קיים קשר לרשומת Delivery
        if (s_dal.Delivery.ReadAll(d => d.CourierId == courierId).Any())
        {
            throw new InvalidOperationException($"Cannot delete Courier {courierId}: courier is associated with existing deliveries (history).");
        }

        // 2. קריאה ל-DAL למחיקה (ה-DAL יזרוק חריגה אם השליח לא קיים)
        try
        {
            s_dal.Courier.Delete(courierId);
        }
        catch (DO.DalDoesNotExistException)
        {
            throw new InvalidOperationException($"Courier with ID {courierId} does not exist and cannot be deleted.");
        }
    }

    /// <summary>
    /// מתודת עזר: מחזירה את הזמן המאוחר ביותר של תחילת או סיום משלוח עבור שליח נתון.
    /// </summary>
    internal static DateTime? GetLastActivityTime(int courierId)
    {
        // נניח ש-using System.Linq; קיים
        var courierDeliveries = s_dal.Delivery.ReadAll(d => d.CourierId == courierId);
        var activityTimes = courierDeliveries
            .SelectMany(d => new[] { d.DeliveryStartTime, d.DeliveryEndTime })
            .Where(dt => dt.HasValue && dt.Value != DateTime.MinValue)
            .ToList();

        return activityTimes.Any() ? activityTimes.Max() : null;
    }

    /// <summary>
    /// מתודה המוזמנת על ידי AdminManager, מעדכנת סטטוס 'לא פעיל' לשליחים שלא היו פעילים.
    /// </summary>
    internal static void PeriodicCourierUpdates(DateTime oldClock, DateTime newClock)
    {
        // נדרש לפתור את שגיאת CS0117 ב-AdminManager
        var allCouriers = s_dal.Courier.ReadAll().ToList();
        TimeSpan inactivityTimeSpan = s_dal.Config.InactivityTimeRange;
        if (inactivityTimeSpan == TimeSpan.Zero) return;

        foreach (var doCourier in allCouriers)
        {
            if (doCourier.IsActive)
            {
                DateTime? lastActivityTime = GetLastActivityTime(doCourier.Id);

                if (lastActivityTime.HasValue && newClock - lastActivityTime.Value > inactivityTimeSpan)
                {
                    s_dal.Courier.Update(doCourier with { IsActive = false });
                }
            }
        }
    }

    /// <summary>
    /// מחשבת את הסטטיסטיקות עבור שליח מסוים (הזמנות בזמן/באיחור).
    /// </summary>
    internal static (int DeliveredOnTime, int DeliveredLate) GetCourierStatistics(int courierId)
    {
        // המתודה הזו שבורה עד למימוש OrderManager.CalculateMaxDeliveryTime
        var completedDeliveries = s_dal.Delivery.ReadAll(d =>
            d.CourierId == courierId &&
            d.OrderClosedStatus == DO.OrderEndStatus.Delivered &&
            d.DeliveryEndTime.HasValue)
            .ToList();

        int deliveredOnTime = 0;
        int deliveredLate = 0;

        foreach (var delivery in completedDeliveries)
        {
            // ** המקום בו הקוד נשבר: OrderManager חסר **
            DateTime maxDeliveryTime = OrderManager.CalculateMaxDeliveryTime(delivery.OrderId);

            if (delivery.DeliveryEndTime <= maxDeliveryTime)
                deliveredOnTime++;
            else
                deliveredLate++;
        }

        return (deliveredOnTime, deliveredLate);
    }

    /// <summary>
    /// מאמת את פרטי כניסת המשתמש (ID וסיסמה) ומזהה את תפקידו.
    /// נדרש שימוש ב-BL.UserRole (אנומרציה פנימית של שכבת ה-BL)
    /// </summary>
    internal static BO.UserRole Login(int userId, string password)
    {
        // 1. בדיקה ראשונה: האם זה המנהל?
        if (userId == s_dal.Config.AdminId)
        {
            if (password == s_dal.Config.AdminPassword)
            {
                return BO.UserRole.Admin;
            }
        }

        // 2. בדיקה שנייה: האם זה שליח?
        try
        {
            // קוראים את השליח מ-DAL
            DO.Courier doCourier = s_dal.Courier.Read(userId);

            if (doCourier.Password == password)
            {
                // מאפשרים כניסה לשליח כל עוד הסיסמה נכונה.
                return BO.UserRole.Courier;
            }
        }
        catch (DO.DalDoesNotExistException) // תופסים את חריגת ה-DAL
        {
            // ממשיכים לשלב 3 לזריקת חריגת כישלון כללית.
        }

        // 3. אם לא המנהל ולא שליח עם סיסמה נכונה
        throw new ArgumentException("Login failed: Invalid ID or password.");
    }
}
