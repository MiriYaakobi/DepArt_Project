using BO;
using System.Collections;
using System.Security.Cryptography;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Web;

namespace Helpers;

/// <summary>
/// Provides utility methods for working with objects and their properties.
/// </summary>
/// <remarks>This class contains methods that utilize reflection to inspect and manipulate object properties. It
/// is designed for use in scenarios such as debugging or logging where detailed object state information is
/// required.</remarks>
internal static class Tools
{
    // Static HttpClient instance for making HTTP requests
    private static readonly HttpClient s_httpClient = new HttpClient();
    // LocationIQ API key for geocoding and routing services
    private const string apiKey = "912f217174197978d7da19cf22005ef920a4772b8c3df456d156f7d7cbb7fea5";

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
    /// We asked AI to create a code for us that calculates the difference between 2 coordinate points.
    /// </summary>
    /// <param name="latitude1">Latitude of point 1</param>
    /// <param name="longitude1">Longitude of point 1</param>
    /// <param name="latitude2">Latitude of point 2</param>
    /// <param name="longitude2">Longitude of point 2</param>
    internal static double GetAirDistance(double latitude1, double longitude1, double latitude2, double longitude2)
    {
        const double R = 6371; // Average radius of the Earth in km

        // Calculate the differences in coordinates in radians
        double diffLatitude = ToRadians(latitude2 - latitude1);
        double diffLongitude = ToRadians(longitude2 - longitude1);

        // Part A of the Haversine formula (calculate the angular distance)
        double a = Math.Sin(diffLatitude / 2) * Math.Sin(diffLatitude / 2) +
                   Math.Cos(ToRadians(latitude1)) * Math.Cos(ToRadians(latitude2)) *
                   Math.Sin(diffLongitude / 2) * Math.Sin(diffLongitude / 2);

        // Part C of the formula (the central angle)
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        // The final distance in km
        return R * c;
    }

    /// <summary>
    /// Gets the geographical coordinates (latitude and longitude) for a given address using the LocationIQ API.
    /// The code to perform the synchronous network read and decode the JSON from the Geocoding/Routing service (LocationIQ)
    /// was created with the help of AI and adapted to the project requirements (use of .Result, exception handling).
    /// </summary>
    internal static (double Latitude, double Longitude)? GetCoordinatesOfAddressSync(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            return null;

        try
        {
            // encoding the address for URL
            string encodedAddress = HttpUtility.UrlEncode(address);

            // Building the LocationIQ API URL (Forward Geocoding)
            string apiUrl = $"https://us1.locationiq.com/v1/search.php?key={apiKey}&q={encodedAddress}&format=json";

            // Performing a synchronous web request using s_httpClient.GetAsync(apiUrl).Result
            // Throws an HttpRequestException if there is a network-level failure (e.g., Timeout)
            HttpResponseMessage response = s_httpClient.GetAsync(apiUrl).Result;

            if (response.IsSuccessStatusCode)
            {
                // Parsing the JSON response
                string resultJson = response.Content.ReadAsStringAsync().Result;

                using (JsonDocument doc = JsonDocument.Parse(resultJson))
                {
                    if (doc.RootElement.ValueKind == JsonValueKind.Array && doc.RootElement.GetArrayLength() > 0)
                    {
                        JsonElement firstResult = doc.RootElement[0];

                        // Extracting the coordinates (lat/lon)
                        if (firstResult.TryGetProperty("lat", out JsonElement latElement) &&
                            firstResult.TryGetProperty("lon", out JsonElement lonElement) &&
                            double.TryParse(latElement.GetString(), out double latitude) &&
                            double.TryParse(lonElement.GetString(), out double longitude))
                        {
                            return (latitude, longitude);
                        }
                    }
                }
            }

            // If the status code was not successful (e.g., 400 Bad Request, 403 Forbidden)
            string errorContent = response.Content.ReadAsStringAsync().Result;
            throw new InvalidOperationException($"Geocoding service returned status code {response.StatusCode} for '{address}'. Error: {errorContent}");
        }
        catch (HttpRequestException ex)
        {
            // Network error (e.g., no connection, Timeout), throw a transient system exception
            throw new InvalidOperationException($"Network error connecting to Geocoding service: {ex.Message}");
        }
        catch (Exception ex) when (ex is JsonException || ex is InvalidOperationException)
        {
            // JSON parsing errors or errors thrown from the API
            throw;
        }
        catch (Exception ex)
        {
            // Unexpected errors
            throw new Exception($"An unexpected error occurred during geocoding: {ex.Message}");
        }
    }

    /// <summary>
    /// gets the actual distance and estimated travel time between two geographical points using the LocationIQ Routing API.
    /// The code to perform the synchronous network read and decode the JSON from the Geocoding/Routing service (LocationIQ)
    /// was created with the help of AI and adapted to the project requirements (use of .Result, exception handling).
    /// </summary>
    /// <param name="startLat"></param>
    /// <param name="startLon"></param>
    /// <param name="endLat"></param>
    /// <param name="endLon"></param>
    /// <param name="shippingType"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="Exception"></exception>
    internal static (double ActualDistance, TimeSpan EstimatedTime)? GetActualDistanceAndEstimatedTimeSync(
          double startLat, double startLon, double endLat, double endLon, BO.DeliveryType shippingType)
    {
        // Using the static instance we defined above: s_httpClient
        var client = s_httpClient;

        // Determine the travel profile for the API based on the shipping type
        string profile;
        switch (shippingType)
        {
            case BO.DeliveryType.Car:
            case BO.DeliveryType.Motorcycle:
                profile = "driving"; // LocationIQ uses "driving" for vehicles
                break;
            case BO.DeliveryType.Bicycle:
                profile = "cycling";
                break;
            case BO.DeliveryType.ByFoot:
                profile = "foot";
                break;
            default:
                throw new ArgumentException($"Unsupported shipping type for routing: {shippingType}.");
        }

        try
        {
            // Building the API URL for LocationIQ Routing
            // The location is in the format: lon1,lat1;lon2,lat2
            string coordinates = $"{startLon},{startLat};{endLon},{endLat}";
            string apiUrl = $"https://us1.locationiq.com/v1/directions/v2/route/{profile}/{coordinates}?key={apiKey}&overview=false";

            // Performing a synchronous network read
            HttpResponseMessage response = client.GetAsync(apiUrl).Result;

            if (response.IsSuccessStatusCode)
            {
                string resultJson = response.Content.ReadAsStringAsync().Result;

                using (JsonDocument doc = JsonDocument.Parse(resultJson))
                {
                    // LocationIQ returns JSON with a "routes" field (array)
                    if (doc.RootElement.TryGetProperty("routes", out JsonElement routesElement) &&
                        routesElement.ValueKind == JsonValueKind.Array && routesElement.GetArrayLength() > 0)
                    {
                        JsonElement route = routesElement[0];

                        if (route.TryGetProperty("distance", out JsonElement distanceElement) &&
                            route.TryGetProperty("duration", out JsonElement durationElement) &&
                            distanceElement.ValueKind == JsonValueKind.Number &&
                            durationElement.ValueKind == JsonValueKind.Number)
                        {
                            // Extracting the data
                            double actualDistanceMeters = distanceElement.GetDouble(); // meters
                            double durationSeconds = durationElement.GetDouble();       // seconds

                            // Unit conversion: meters to kilometers, seconds to TimeSpan
                            double actualDistanceKm = actualDistanceMeters / 1000.0;
                            TimeSpan estimatedTime = TimeSpan.FromSeconds(durationSeconds);

                            return (actualDistanceKm, estimatedTime); // sending back the tuple
                        }
                    }
                }
            }

            // If we reach here, the API did not return a successful route
            string errorContent = response.Content.ReadAsStringAsync().Result;
            throw new InvalidOperationException($"Routing service failed to find a route. Status: {response.StatusCode}. Error: {errorContent}");
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException($"Network error connecting to Routing service: {ex.Message}");
        }
        catch (Exception ex)
        {
            throw new Exception($"An unexpected error occurred during routing: {ex.Message}");
        }
    }

    /// <summary>
    /// a function that hashes a password using SHA256.
    /// </summary>
    /// <param name="password"></param>
    /// <returns></returns>
    /// <exception cref="BlInvalidDataException"></exception>
    internal static string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
        {
            throw new BlInvalidDataException("Password cannot be empty");
        }

        //use SHA256 to hash the password
        using (SHA256 sha256Hash = SHA256.Create())
        {
            //Convert the password to byte array and computing the hash
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));

            // Convert byte array to a hexadecimal string for saving in the database
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                //format each byte as a two-digit hexadecimal string
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();
        }
    }

    /// <summary>
    /// verifies if the entered password matches the stored hashed password.
    /// </summary>
    /// <param name="enteredPassword"></param>
    /// <param name="storedHash"></param>
    /// <returns></returns>
    internal static bool VerifyPassword(string enteredPassword, string storedHash)
    {
        string hashedEnteredPassword = HashPassword(enteredPassword);

        //compare the hashed entered password with the stored hash
        return hashedEnteredPassword == storedHash;
    }
}