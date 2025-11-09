namespace Dal;

/// <summary>
/// class to hold configuration settings and provide unique identifiers for deliveries and orders.
/// </summary>
static internal class Config
{
    // Delivery
    internal const int StartDeliveryId = 1000;
    private static int nextDeliveryId = StartDeliveryId;
    internal static int NextDeliveryId { get => nextDeliveryId++; }

    // Order
    internal const int StartOrderId = 1000;
    private static int nextOrderId = StartOrderId;
    internal static int NextOrderId { get => nextOrderId++; }

    // Other Configurations
    internal static DateTime Clock { get; set; } = DateTime.Now;
    internal static int AdminId { get; set; } = 123456782;
    internal static string AdminPassword { get; set; } = "Deafult1234$";
    internal static string? CompenyAddress { get; set; } = null;
    internal static double? CompenyLatitude { get; set; } = null;
    internal static double? CompenyLongitude { get; set; } = null;
    internal static double? DeliveryMaxDistance { get; set; } = null;
    internal static double AverageVehicleSpeedKmH { get; set; } = 0.0;
    internal static double AverageMotorcycleSpeedKmH { get; set; } = 0.0;
    internal static double AverageBicycleSpeedKmH { get; set; } = 0.0;
    internal static double AverageByFootSpeedKmH { get; set; } = 0.0;
    internal static TimeSpan MaxDeliveryRange { get; set; } = TimeSpan.Zero;
    internal static TimeSpan RiskRange { get; set; } = TimeSpan.Zero;
    internal static TimeSpan InactivityTimeRange { get; set; } = TimeSpan.Zero;

    /// <summary>
    /// resets all configuration settings to their default values.
    /// </summary>
    internal static void Reset()
    {
        nextDeliveryId = StartDeliveryId;
        nextOrderId = StartOrderId;
        Clock = DateTime.Now;
        AdminId = 123456782;
        AdminPassword = "Deafult1234$";
        CompenyAddress = null;
        CompenyLatitude = 32.0853;
        CompenyLongitude = 34.7818;
        DeliveryMaxDistance = null;
        AverageVehicleSpeedKmH = 0.0;
        AverageMotorcycleSpeedKmH = 0.0;
        AverageBicycleSpeedKmH = 0.0;
        AverageByFootSpeedKmH = 0.0;
        MaxDeliveryRange = TimeSpan.Zero;
        RiskRange = TimeSpan.Zero;
        InactivityTimeRange = TimeSpan.Zero;
    }
}

