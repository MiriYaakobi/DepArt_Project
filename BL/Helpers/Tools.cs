using System.Collections;
using System.Security.Cryptography;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Web;
using System.Collections.Concurrent;

namespace Helpers;

/// <summary>
/// Provides utility methods for working with objects and their properties.
/// </summary>
/// <remarks>This class contains methods that utilize reflection to inspect and manipulate object properties. It
/// is designed for use in scenarios such as debugging or logging where detailed object state information is
/// required.</remarks>
internal static class Tools
{
    // Optimization: Cache for routing requests to prevent duplicate network calls
    // Key: string (composed of type + coordinates), Value: Tuple of distance and time
    private static readonly ConcurrentDictionary<string, (double Distance, TimeSpan Time)> s_routeCache = new();

    // Static HttpClient instance for making HTTP requests
    private static readonly HttpClient s_httpClient = new HttpClient();

    /// <summary>
    /// generates a string representation of an object's public properties and their values using reflection.
    /// When writing this function, we used AI to ensure that the logic and sorting order were correct.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="t"></param>
    /// <returns></returns>
    public static string ToStringProperty<T>(this T t)
    {
        // Handle null case
        if (t == null)
            return "Object is null";

        Type type = t.GetType(); // Get the type of the object
        StringBuilder sb = new StringBuilder(); // StringBuilder for efficient string concatenation

        // Header for the details
        sb.AppendLine($"\n\n--- {type.Name} Details ---");

        // Get all public properties of the object
        PropertyInfo[] properties = type.GetProperties();

        // Iterate through each property and get its value
        foreach (PropertyInfo property in properties)
        {
            // Get the value of the property
            object? value = property.GetValue(t);

            // Handle null values
            if (value == null)
                sb.AppendLine($"    {property.Name}: *****");

            //check if the property is a collection
            else if (value is IEnumerable enumerable && value is not string)
            {
                //get the count of items in the collection
                int count = (enumerable is ICollection collection) ? collection.Count : enumerable.Cast<object>().Count();

                //create a collection representation
                sb.AppendLine($"    {property.Name}: (Collection of {property.PropertyType.GetGenericArguments().FirstOrDefault()?.Name} - Count: {((ICollection)enumerable).Count})");

                int itemIndex = 0;

                //iterate through the collection items
                foreach (var item in enumerable)
                {
                    sb.AppendLine($"        [{itemIndex}]: {item}");
                    itemIndex++;
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
    /// converts degrees to radians.
    /// </summary>
    /// <param name="degrees"></param>
    /// <returns></returns>
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
    /// was created with the help of AI and adapted to the project requirements (use of .Result, exception handling)
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    /// <exception cref="BO.BlInvalidOperationException"></exception>
    internal static (double Latitude, double Longitude)? GetCoordinatesOfAddressSync(string address)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(address))
            return null;

        // Use default API key if none provided
        string effectiveKey = Secrets.LocationIqApiKey;

        try
        {
            // encoding the address for URL
            string encodedAddress = HttpUtility.UrlEncode(address);

            // Building the LocationIQ API URL (Forward Geocoding)
            string apiUrl = $"https://us1.locationiq.com/v1/search.php?key={effectiveKey}&q={encodedAddress}&format=json";

            // Performing a synchronous web request using s_httpClient.GetAsync(apiUrl).Result
            // Throws an HttpRequestException if there is a network-level failure (e.g., Timeout)
            HttpResponseMessage response = s_httpClient.GetAsync(apiUrl).Result;

            if (response.IsSuccessStatusCode)
            {
                // Parsing the JSON response
                string resultJson = response.Content.ReadAsStringAsync().Result;

                // The response is expected to be a JSON array of results
                using (JsonDocument doc = JsonDocument.Parse(resultJson))
                {
                    // Check if we have at least one result
                    if (doc.RootElement.ValueKind == JsonValueKind.Array && doc.RootElement.GetArrayLength() > 0)
                    {
                        JsonElement firstResult = doc.RootElement[0];

                        // Extracting the coordinates (lat/lon)
                        if (firstResult.TryGetProperty("lat", out JsonElement latElement) &&
                            firstResult.TryGetProperty("lon", out JsonElement lonElement) &&
                            double.TryParse(latElement.GetString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double latitude) &&
                            double.TryParse(lonElement.GetString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double longitude))
                        {
                            return (latitude, longitude);
                        }
                    }
                }
            }

            // If the status code was not successful (e.g., 400 Bad Request, 403 Forbidden)
            string errorContent = response.Content.ReadAsStringAsync().Result;
            throw new BO.BlInvalidOperationException($"ERROR: Geocoding service returned status code {response.StatusCode} for '{address}'. Error: {errorContent}");
        }
        catch (HttpRequestException ex)
        {
            throw new BO.BlInvalidOperationException($"Network error connecting to Geocoding service: {ex.Message}", ex);
        }
        catch (Exception ex) when (ex is System.Text.Json.JsonException || ex is InvalidOperationException)
        {
            throw new BO.BlInvalidOperationException($"Failed to parse Geocoding response: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new BO.BlInvalidOperationException($"An unexpected error occurred during geocoding: {ex.Message}", ex);
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
        string effectiveKey = Secrets.LocationIqApiKey;

        // Determine the travel profile for the API based on the shipping type
        string profile = shippingType switch
        {
            BO.DeliveryType.Car or BO.DeliveryType.Motorcycle => "driving",
            BO.DeliveryType.Bicycle => "cycling",
            _ => "walking"
        };

        try
        {
            // Building the API URL for LocationIQ Routing
            // The location is in the format: lon1,lat1;lon2,lat2
            string coordinates = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                        "{0},{1};{2},{3}", startLon, startLat, endLon, endLat);

            // Constructing the full API URL
            string apiUrl = $"https://us1.locationiq.com/v1/directions/driving/{coordinates}?key={effectiveKey}&overview=false";

            // Replace "driving" with the appropriate profile if needed
            if (profile != "driving")
                apiUrl = apiUrl.Replace("driving", profile);

            // Performing a synchronous network read
            HttpResponseMessage response = s_httpClient.GetAsync(apiUrl).Result;

            // Checking for successful response
            if (response.IsSuccessStatusCode)
            {
                string resultJson = response.Content.ReadAsStringAsync().Result;

                // Parsing the JSON response
                using (JsonDocument doc = JsonDocument.Parse(resultJson))
                {
                    // LocationIQ returns JSON with a "routes" field (array)
                    if (doc.RootElement.TryGetProperty("routes", out JsonElement routesElement) &&
                        routesElement.ValueKind == JsonValueKind.Array && routesElement.GetArrayLength() > 0)
                    {
                        JsonElement route = routesElement[0];

                        // Extracting distance and duration
                        if (route.TryGetProperty("distance", out JsonElement distanceElement) &&
                            route.TryGetProperty("duration", out JsonElement durationElement) &&
                            distanceElement.ValueKind == JsonValueKind.Number &&
                            durationElement.ValueKind == JsonValueKind.Number)
                        {
                            // Extracting the data
                            double actualDistanceMeters = distanceElement.GetDouble(); // meters
                            double durationSeconds = durationElement.GetDouble(); // seconds

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
            //throw new InvalidOperationException($"Routing service failed to find a route. Status: {response.StatusCode}. Error: {errorContent}");
            throw new BO.BlInvalidOperationException($"Routing API failed. Status: {response.StatusCode}. Details: {errorContent}");
        }
        catch (HttpRequestException /*ex*/)
        {
            return null;
            //throw new BO.BlInvalidOperationException($"Network error connecting to Routing service: {ex.Message}", ex);
        }
        catch (Exception /*ex*/)
        {
            return null;
            //throw new BO.BlInvalidOperationException($"An unexpected error occurred during routing: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// a function that hashes a password using SHA256.
    /// </summary>
    /// <param name="password"></param>
    /// <returns></returns>
    /// <exception cref="BlInvalidDataException"></exception>
    public static string HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
        {
            throw new BO.BlInvalidDataException("Password cannot be empty");
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


    /// <summary>
    /// ASYNC version: Gets the geographical coordinates for a given address.
    /// </summary>
    internal static async Task<(double Latitude, double Longitude)?> GetCoordinatesOfAddressAsync(string address)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(address))
            return null;

        string effectiveKey = Secrets.LocationIqApiKey;

        try
        {
            string encodedAddress = HttpUtility.UrlEncode(address);
            string apiUrl = $"https://us1.locationiq.com/v1/search.php?key={effectiveKey}&q={encodedAddress}&format=json";

            // asynchronous web request
            HttpResponseMessage response = await s_httpClient.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                // asynchronous read of the content
                string resultJson = await response.Content.ReadAsStringAsync();

                using (JsonDocument doc = JsonDocument.Parse(resultJson))
                {
                    if (doc.RootElement.ValueKind == JsonValueKind.Array && doc.RootElement.GetArrayLength() > 0)
                    {
                        JsonElement firstResult = doc.RootElement[0];
                        if (firstResult.TryGetProperty("lat", out JsonElement latElement) &&
                            firstResult.TryGetProperty("lon", out JsonElement lonElement) &&
                            double.TryParse(latElement.GetString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double latitude) &&
                            double.TryParse(lonElement.GetString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double longitude))
                        {
                            return (latitude, longitude);
                        }
                    }
                }
            }
            // If the status code was not successful
            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// ASYNC version with CACHING: Gets actual distance and estimated time.
    /// </summary>
    internal static async Task<(double ActualDistance, TimeSpan EstimatedTime)?> GetActualDistanceAndEstimatedTimeAsync(
          double startLat, double startLon, double endLat, double endLon, BO.DeliveryType shippingType)
    {
        // create a unique cache key based on input parameters
        string cacheKey = $"{shippingType}|{startLat}|{startLon}|{endLat}|{endLon}";

        // Check if the result already exists in memory (Cache)
        if (s_routeCache.TryGetValue(cacheKey, out var cachedResult))
        {
            // We saved a network request. We'll return what we saved last time.
            return cachedResult;
        }

        // If we reached here, the result is not in memory. We need to query the internet 

        string effectiveKey = Secrets.LocationIqApiKey;
        
        string profile = shippingType switch
        {
            BO.DeliveryType.Car or BO.DeliveryType.Motorcycle => "driving",
            BO.DeliveryType.Bicycle => "cycling",
            _ => "walking"
        };

        try
        {
            string coordinates = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                        "{0},{1};{2},{3}", startLon, startLat, endLon, endLat);

            string apiUrl = $"https://us1.locationiq.com/v1/directions/driving/{coordinates}?key={effectiveKey}&overview=false";

            if (profile != "driving")
                apiUrl = apiUrl.Replace("driving", profile);

            HttpResponseMessage response = await s_httpClient.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                string resultJson = await response.Content.ReadAsStringAsync();

                using (JsonDocument doc = JsonDocument.Parse(resultJson))
                {
                    if (doc.RootElement.TryGetProperty("routes", out JsonElement routesElement) &&
                        routesElement.ValueKind == JsonValueKind.Array && routesElement.GetArrayLength() > 0)
                    {
                        JsonElement route = routesElement[0];
                        if (route.TryGetProperty("distance", out JsonElement distanceElement) &&
                            route.TryGetProperty("duration", out JsonElement durationElement))
                        {
                            double actualDistanceMeters = distanceElement.GetDouble();
                            double durationSeconds = durationElement.GetDouble();

                            double actualDistanceKm = actualDistanceMeters / 1000.0;
                            TimeSpan estimatedTime = TimeSpan.FromSeconds(durationSeconds);

                            var result = (actualDistanceKm, estimatedTime);

                            // Store the result in the cache for future requests
                            s_routeCache.TryAdd(cacheKey, result);

                            return result;
                        }
                    }
                }
            }
            return null;
        }
        catch
        {
            return null;
        }
    }
}