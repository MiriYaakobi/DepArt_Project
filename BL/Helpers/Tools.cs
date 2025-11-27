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
}