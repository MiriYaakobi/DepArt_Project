using System.Runtime.CompilerServices;

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
    [MethodImpl(MethodImplOptions.Synchronized)]
    internal static void Reset()
    {
        nextDeliveryId = NextDeliveryId;
        nextOrderId = NextOrderId;

        Clock = DateTime.Now;

        AdminId = 123456782;
        AdminPassword = "Deafult1234$";

        CompenyAddress = "Ahad Ha'am St 9 Tel-Aviv";
        CompenyLatitude = 32.0641632;
        CompenyLongitude = 34.7692375;

        DeliveryMaxDistance = 300.0;

        AverageVehicleSpeedKmH = 80.0;
        AverageMotorcycleSpeedKmH = 100.0;
        AverageBicycleSpeedKmH = 20.0;
        AverageByFootSpeedKmH = 5.0;

        MaxDeliveryRange = TimeSpan.FromDays(14);
        RiskRange = TimeSpan.FromHours(24);
        InactivityTimeRange = TimeSpan.FromDays(30);
    }
}