namespace Dal;

internal static class Config
{
    internal const string s_data_config_xml = "data-config.xml";
    internal const string s_couriers_xml = "couriers.xml";
    internal const string s_deliveries_xml = "deliveries.xml";
    internal const string s_orders_xml = "orders.xml";

    internal static int NextDeliveryId
    {
        get => XMLTools.GetAndIncreaseConfigIntVal(s_data_config_xml, "NextDeliveryId");
        private set => XMLTools.SetConfigIntVal(s_data_config_xml, "NextDeliveryId", value);
    }

    internal static int NextOrderId
    {
        get => XMLTools.GetAndIncreaseConfigIntVal(s_data_config_xml, "NextOrderId");
        private set => XMLTools.SetConfigIntVal(s_data_config_xml, "NextOrderId", value);
    }

    internal static DateTime Clock
    {
        get => XMLTools.GetConfigDateVal(s_data_config_xml, "Clock");
        set => XMLTools.SetConfigDateVal(s_data_config_xml, "Clock", value);
    }

    internal static int AdminId
    {
        get => XMLTools.GetConfigIntVal(s_data_config_xml, "AdminId");
        set => XMLTools.SetConfigIntVal(s_data_config_xml, "AdminId", value);
    }
    internal static string AdminPassword
    {
        get => XMLTools.GetConfigStringVal(s_data_config_xml, "AdminPassword");
        set => XMLTools.SetConfigStringVal(s_data_config_xml, "AdminPassword", value);
    }
    internal static string? CompenyAddress
    {
        get => XMLTools.GetConfigNullableStringVal(s_data_config_xml, "CompenyAddress");
        set => XMLTools.SetConfigNullableStringVal(s_data_config_xml, "CompenyAddress", value);
    }
    internal static double? CompenyLatitude
    {
        get => XMLTools.GetConfigNullableDoubleVal(s_data_config_xml, "CompenyLatitude");
        set => XMLTools.SetConfigNullableDoubleVal(s_data_config_xml, "CompenyLatitude", value);
    }
    internal static double? CompenyLongitude
    {
        get => XMLTools.GetConfigNullableDoubleVal(s_data_config_xml, "CompenyLongitude");
        set => XMLTools.SetConfigNullableDoubleVal(s_data_config_xml, "CompenyLongitude", value);
    }
    internal static double? DeliveryMaxDistance
    {
        get => XMLTools.GetConfigNullableDoubleVal(s_data_config_xml, "DeliveryMaxDistance");
        set => XMLTools.SetConfigNullableDoubleVal(s_data_config_xml, "DeliveryMaxDistance", value);
    }
    internal static double AverageVehicleSpeedKmH
    {
        get => XMLTools.GetConfigDoubleVal(s_data_config_xml, "AverageVehicleSpeedKmH");
        set => XMLTools.SetConfigDoubleVal(s_data_config_xml, "AverageVehicleSpeedKmH", value);
    }
    internal static double AverageMotorcycleSpeedKmH
    {
        get => XMLTools.GetConfigDoubleVal(s_data_config_xml, "AverageMotorcycleSpeedKmH");
        set => XMLTools.SetConfigDoubleVal(s_data_config_xml, "AverageMotorcycleSpeedKmH", value);
    }
    internal static double AverageBicycleSpeedKmH
    {
        get => XMLTools.GetConfigDoubleVal(s_data_config_xml, "AverageBicycleSpeedKmH");
        set => XMLTools.SetConfigDoubleVal(s_data_config_xml, "AverageBicycleSpeedKmH", value);
    }
    internal static double AverageByFootSpeedKmH
    {
        get => XMLTools.GetConfigDoubleVal(s_data_config_xml, "AverageByFootSpeedKmH");
        set => XMLTools.SetConfigDoubleVal(s_data_config_xml, "AverageByFootSpeedKmH", value);
    }
    internal static TimeSpan MaxDeliveryRange
    {
        get => XMLTools.GetConfigTimeSpanVal(s_data_config_xml, "MaxDeliveryRange");
        set => XMLTools.SetConfigTimeSpanVal(s_data_config_xml, "MaxDeliveryRange", value);
    }
    internal static TimeSpan RiskRange
    {
        get => XMLTools.GetConfigTimeSpanVal(s_data_config_xml, "RiskRange");
        set => XMLTools.SetConfigTimeSpanVal(s_data_config_xml, "RiskRange", value);
    }
    internal static TimeSpan InactivityTimeRange
    {
        get => XMLTools.GetConfigTimeSpanVal(s_data_config_xml, "InactivityTimeRange");
        set => XMLTools.SetConfigTimeSpanVal(s_data_config_xml, "InactivityTimeRange", value);
    }

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