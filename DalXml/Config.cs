namespace Dal;

internal static class Config
{
    internal const string s_data_config_xml = "data-config.xml";
    internal const string s_couriers_xml = "couriers.xml";
    internal const string s_deliveries_xml = "deliveries.xml";
    internal const string s_orders_xml = "orders.xml";

    internal static int nextDeliveryId
    {
        get => XMLTools.GetAndIncreaseConfigIntVal(s_data_config_xml, "nextDeliveryId");
        private set => XMLTools.SetConfigIntVal(s_data_config_xml, "nextDeliveryId", value);
    }

    internal static int nextOrderId
    {
        get => XMLTools.GetAndIncreaseConfigIntVal(s_data_config_xml, "nextOrderId");
        private set => XMLTools.SetConfigIntVal(s_data_config_xml, "nextOrderId", value);
    }

    internal static DateTime Clock
    {
        get => XMLTools.GetConfigDateVal(s_data_config_xml, "clock");
        set => XMLTools.SetConfigDateVal(s_data_config_xml, "clock", value);
    }

    internal static int AdminId
    {
        get => XMLTools.GetConfigIntVal(s_data_config_xml, "adminId");
        set => XMLTools.SetConfigIntVal(s_data_config_xml, "adminId", value);
    }
    internal static string AdminPassword
    {
        get => XMLTools.GetConfigStringVal(s_data_config_xml, "adminPassword");
        set => XMLTools.SetConfigStringVal(s_data_config_xml, "adminPassword", value);
    }
    internal static string? CompenyAddress
    { 
        get => XMLTools.GetConfigNullableStringVal(s_data_config_xml, "compenyAddress");
        set => XMLTools.SetConfigNullableStringVal(s_data_config_xml, "compenyAddress", value);
    }
    internal static double? CompenyLatitude
    {
        get => XMLTools.GetConfigNullableDoubleVal(s_data_config_xml, "compenyLatitude");
        set => XMLTools.SetConfigNullableDoubleVal(s_data_config_xml, "compenyLatitude", value);
    }
    internal static double? CompenyLongitude
    {
        get => XMLTools.GetConfigNullableDoubleVal(s_data_config_xml, "compenyLongitude");
        set => XMLTools.SetConfigNullableDoubleVal(s_data_config_xml, "compenyLongitude", value);
    }
    internal static double? DeliveryMaxDistance
    {
        get => XMLTools.GetConfigNullableDoubleVal(s_data_config_xml, "deliveryMaxDistance");
        set => XMLTools.SetConfigNullableDoubleVal(s_data_config_xml, "deliveryMaxDistance", value);
    }
    internal static double AverageVehicleSpeedKmH
    {
        get => XMLTools.GetConfigDoubleVal(s_data_config_xml, "averageVehicleSpeedKmH");
        set => XMLTools.SetConfigDoubleVal(s_data_config_xml, "averageVehicleSpeedKmH", value);
    }
    internal static double AverageMotorcycleSpeedKmH
    {
        get => XMLTools.GetConfigDoubleVal(s_data_config_xml, "averageMotorcycleSpeedKmH");
        set => XMLTools.SetConfigDoubleVal(s_data_config_xml, "averageMotorcycleSpeedKmH", value);
    }
    internal static double AverageBicycleSpeedKmH
    { 
        get => XMLTools.GetConfigDoubleVal(s_data_config_xml, "averageBicycleSpeedKmH");
        set => XMLTools.SetConfigDoubleVal(s_data_config_xml, "averageBicycleSpeedKmH", value);
    }
    internal static double AverageByFootSpeedKmH
    {
        get => XMLTools.GetConfigDoubleVal(s_data_config_xml, "averageByFootSpeedKmH");
        set => XMLTools.SetConfigDoubleVal(s_data_config_xml, "averageByFootSpeedKmH", value);
    }
    internal static TimeSpan MaxDeliveryRange
    {
        get => XMLTools.GetConfigTimeSpanVal(s_data_config_xml, "maxDeliveryRange");
        set => XMLTools.SetConfigTimeSpanVal(s_data_config_xml, "maxDeliveryRange", value);
    }
    internal static TimeSpan RiskRange
    {
        get => XMLTools.GetConfigTimeSpanVal(s_data_config_xml, "riskRange");
        set => XMLTools.SetConfigTimeSpanVal(s_data_config_xml, "riskRange", value);
    }
    internal static TimeSpan InactivityTimeRange
    { 
        get => XMLTools.GetConfigTimeSpanVal(s_data_config_xml, "inactivityTimeRange");
        set => XMLTools.SetConfigTimeSpanVal(s_data_config_xml, "inactivityTimeRange", value);
    }

    internal static void Reset()
    {
        nextDeliveryId = 1000;
        nextOrderId = 1000;
        Clock = DateTime.Now;
        AdminId = 123456782;
        AdminPassword = "Deafult1234$";
        CompenyAddress = "Ahad Ha'am St 9 Tel-Aviv";
        CompenyLatitude = 32.0641632;
        CompenyLongitude = 34.7692375;
        DeliveryMaxDistance = 286;
        AverageVehicleSpeedKmH = 0.0;
        AverageMotorcycleSpeedKmH = 0.0;
        AverageBicycleSpeedKmH = 0.0;
        AverageByFootSpeedKmH = 0.0;
        MaxDeliveryRange = TimeSpan.Zero;
        RiskRange = TimeSpan.Zero;
        InactivityTimeRange = TimeSpan.Zero;
    }
}
