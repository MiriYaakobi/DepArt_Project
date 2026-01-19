using System.Runtime.CompilerServices;

namespace Dal;

internal static class Config
{
    internal const string s_data_config_xml = "data-config.xml";
    internal const string s_couriers_xml = "couriers.xml";
    internal const string s_deliveries_xml = "deliveries.xml";
    internal const string s_orders_xml = "orders.xml";

    internal static int NextDeliveryId
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => XMLTools.GetAndIncreaseConfigIntVal(s_data_config_xml, "NextDeliveryId");

        [MethodImpl(MethodImplOptions.Synchronized)]
        private set => XMLTools.SetConfigIntVal(s_data_config_xml, "NextDeliveryId", value);
    }

    internal static int NextOrderId
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => XMLTools.GetAndIncreaseConfigIntVal(s_data_config_xml, "NextOrderId");

        [MethodImpl(MethodImplOptions.Synchronized)]
        private set => XMLTools.SetConfigIntVal(s_data_config_xml, "NextOrderId", value);
    }

    internal static DateTime Clock
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => XMLTools.GetConfigDateVal(s_data_config_xml, "Clock");

        [MethodImpl(MethodImplOptions.Synchronized)]
        set => XMLTools.SetConfigDateVal(s_data_config_xml, "Clock", value);
    }

    internal static int AdminId
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => XMLTools.GetConfigIntVal(s_data_config_xml, "AdminId");

        [MethodImpl(MethodImplOptions.Synchronized)]
        set => XMLTools.SetConfigIntVal(s_data_config_xml, "AdminId", value);
    }
    internal static string AdminPassword
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => XMLTools.GetConfigStringVal(s_data_config_xml, "AdminPassword");

        [MethodImpl(MethodImplOptions.Synchronized)]
        set => XMLTools.SetConfigStringVal(s_data_config_xml, "AdminPassword", value);
    }
    internal static string? CompenyAddress
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => XMLTools.GetConfigNullableStringVal(s_data_config_xml, "CompenyAddress");

        [MethodImpl(MethodImplOptions.Synchronized)]
        set => XMLTools.SetConfigNullableStringVal(s_data_config_xml, "CompenyAddress", value);
    }
    internal static double? CompenyLatitude
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => XMLTools.GetConfigNullableDoubleVal(s_data_config_xml, "CompenyLatitude");

        [MethodImpl(MethodImplOptions.Synchronized)]
        set => XMLTools.SetConfigNullableDoubleVal(s_data_config_xml, "CompenyLatitude", value);
    }
    internal static double? CompenyLongitude
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => XMLTools.GetConfigNullableDoubleVal(s_data_config_xml, "CompenyLongitude");

        [MethodImpl(MethodImplOptions.Synchronized)]
        set => XMLTools.SetConfigNullableDoubleVal(s_data_config_xml, "CompenyLongitude", value);
    }
    internal static double? DeliveryMaxDistance
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => XMLTools.GetConfigNullableDoubleVal(s_data_config_xml, "DeliveryMaxDistance");

        [MethodImpl(MethodImplOptions.Synchronized)]
        set => XMLTools.SetConfigNullableDoubleVal(s_data_config_xml, "DeliveryMaxDistance", value);
    }
    internal static double AverageVehicleSpeedKmH
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => XMLTools.GetConfigDoubleVal(s_data_config_xml, "AverageVehicleSpeedKmH");

        [MethodImpl(MethodImplOptions.Synchronized)]
        set => XMLTools.SetConfigDoubleVal(s_data_config_xml, "AverageVehicleSpeedKmH", value);
    }
    internal static double AverageMotorcycleSpeedKmH
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => XMLTools.GetConfigDoubleVal(s_data_config_xml, "AverageMotorcycleSpeedKmH");

        [MethodImpl(MethodImplOptions.Synchronized)]
        set => XMLTools.SetConfigDoubleVal(s_data_config_xml, "AverageMotorcycleSpeedKmH", value);
    }
    internal static double AverageBicycleSpeedKmH
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => XMLTools.GetConfigDoubleVal(s_data_config_xml, "AverageBicycleSpeedKmH");

        [MethodImpl(MethodImplOptions.Synchronized)]
        set => XMLTools.SetConfigDoubleVal(s_data_config_xml, "AverageBicycleSpeedKmH", value);
    }
    internal static double AverageByFootSpeedKmH
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => XMLTools.GetConfigDoubleVal(s_data_config_xml, "AverageByFootSpeedKmH");

        [MethodImpl(MethodImplOptions.Synchronized)]
        set => XMLTools.SetConfigDoubleVal(s_data_config_xml, "AverageByFootSpeedKmH", value);
    }
    internal static TimeSpan MaxDeliveryRange
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => XMLTools.GetConfigTimeSpanVal(s_data_config_xml, "MaxDeliveryRange");

        [MethodImpl(MethodImplOptions.Synchronized)]
        set => XMLTools.SetConfigTimeSpanVal(s_data_config_xml, "MaxDeliveryRange", value);
    }
    internal static TimeSpan RiskRange
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => XMLTools.GetConfigTimeSpanVal(s_data_config_xml, "RiskRange");

        [MethodImpl(MethodImplOptions.Synchronized)]
        set => XMLTools.SetConfigTimeSpanVal(s_data_config_xml, "RiskRange", value);
    }
    internal static TimeSpan InactivityTimeRange
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => XMLTools.GetConfigTimeSpanVal(s_data_config_xml, "InactivityTimeRange");

        [MethodImpl(MethodImplOptions.Synchronized)]
        set => XMLTools.SetConfigTimeSpanVal(s_data_config_xml, "InactivityTimeRange", value);
    }


    /// <summary>
    /// resets all configuration values to their default settings.
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
    internal static void Reset()
    {
        NextDeliveryId = 1000;
        NextOrderId = 1000;

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