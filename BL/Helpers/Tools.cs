using System.Collections;
using System.Reflection;
using System.Text;

namespace Helpers;

/// <summary>
/// Provides utility methods for working with objects and their properties.
/// </summary>
/// <remarks>This class contains methods that utilize reflection to inspect and manipulate object properties. It
/// is designed for use in scenarios such as debugging or logging where detailed object state information is
/// required.</remarks>
internal static class Tools
{
    /// <summary>
    /// Generates a string representation of the properties and their values for the specified object.
    /// In writing this method, we used AI for accurate and correct use of Reflection.
    /// </summary>
    /// <remarks>This method uses reflection to retrieve the properties of the object.  It is suitable for
    /// debugging or logging purposes where a quick overview of an object's state is needed.</remarks>
    /// <typeparam name="T">The type of the object whose properties are to be represented as a string.</typeparam>
    /// <param name="t">The object instance to be converted to a string. Cannot be null.</param>
    /// <returns>A string containing the names and values of the properties of the object.  If the object is null, returns
    /// "Object is null".</returns>
    public static string ToStringProperty<T>(this T t)
    {
        // Handle null case
        if (t == null)
            return "Object is null";

        Type type = t.GetType(); // Get the type of the object
        StringBuilder sb = new StringBuilder(); // StringBuilder for efficient string concatenation

        // Header for the details
        sb.AppendLine($"--- {type.Name} Details ---");

        // Get all public properties of the object
        PropertyInfo[] properties = type.GetProperties();

        // Iterate through each property and get its value
        foreach (PropertyInfo property in properties)
        {
            // Get the value of the property
            object? value = property.GetValue(t);

            // Handle null values
            if (value == null)
                sb.AppendLine($"    {property.Name}: null");

            //check if the property is a collection
            if (value is IEnumerable enumerable && value is not string)
            {
                //create a collection representation
                sb.AppendLine($"    {property.Name}: (Collection of {property.PropertyType.GetGenericArguments().FirstOrDefault()?.Name} - Count: {((ICollection)enumerable).Count})");

                int count = 0;

                //iterate through the collection items
                foreach (var item in enumerable)
                {
                    sb.AppendLine($"        [{count}]: {item}");
                    count++;
                }
            }

            // Regular property
            else
                sb.AppendLine($"    {property.Name}: {value}");
        }

        // end of details
        sb.AppendLine("--------------------------");
        return sb.ToString();
    }

    /// <summary>
    /// Converts degrees to radians.
    /// </summary>
    private static double ToRadians(double degrees)
    {
        return degrees * (Math.PI / 180);
    }

    /// <summary>
    /// Calculates the air distance between two geographical points using the Haversine formula.
    /// </summary>
    /// <param name="lat1">Latitude of point 1</param>
    /// <param name="lon1">Longitude of point 1</param>
    /// <param name="lat2">Latitude of point 2</param>
    /// <param name="lon2">Longitude of point 2</param>
    internal static double GetAirDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371; // Average radius of the Earth in km

        // Calculate the differences in coordinates in radians
        double dLat = ToRadians(lat2 - lat1);
        double dLon = ToRadians(lon2 - lon1);

        // Part A of the Haversine formula (calculate the angular distance)
        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        // Part C of the formula (the central angle)
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        // The final distance in km
        return R * c;
    }

    /// <summary>
    /// מתודה סינכרונית לקבלת קואורדינטות מכתובת טקסטואלית, באמצעות שירות חיצוני.
    /// בשלב זה, היא ממומשת כהחזרת ערך מדומה לצורך בדיקות.
    /// </summary>
    /// <param name="address">כתובת טקסטואלית</param>
    /// <returns>טופל (Latitude, Longitude) או null אם הכתובת לא נמצאה.</returns>
    internal static (double Latitude, double Longitude)? GetCoordinatesOfAddressSync(string address)
    {
        // *** קוד פניית רשת חיצונית לאתר Geocoding אמור להיות כאן ***

        // כרגע, לצורך המימוש הלוגי (דאגה לזריקת חריגות וטיפול ב-null):
        if (string.IsNullOrWhiteSpace(address) || address.Contains("Invalid"))
            return null; // כתובת לא נמצאה

        // דוגמה לכתובת חוקית - מחזירה קואורדינטות קבועות
        if (address.Contains("Default"))
        {
            return (32.0641632, 34.7692375); // קואורדינטות דמיוניות
        }

        // אם לא כתובת ברירת מחדל, מחזירים ערך דמה כללי
        return (32.123456, 34.567890);
    }
}