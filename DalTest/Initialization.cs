namespace DalTest;
using DalApi;
using DO;
using System;

public static class Initialization
{
    private static ICourier? s_dalCourier;
    private static IOrder? s_dalOrder;
    private static IDelivery? s_dalDelivery;
    private static IConfig? s_dalConfig;

    private static readonly Random s_rand = new();

    private static void createCouriers()
    {

    }

    private static void createOrders()
    {
        var addresses = new (string Address, double Lat, double Lon)[]
        {
            ("Herzl 10, Tel Aviv", 32.0675, 34.7775),
            ("Jaffa Road 2, Jerusalem", 31.7780, 35.2345),
            ("Ha'arbaa 14, Herzliya", 32.1632, 34.8406),
            ("Begin 101, Petah Tikva", 32.0880, 34.8875),
            ("Sderot Hen 5, Haifa", 32.8040, 34.9896),
            ("Ha'atzmaut 25, Bat Yam", 32.0210, 34.7544),
            ("Modi'in", 31.8989, 35.0078),
            ("Ashdod", 31.8044, 34.6553),
            ("Beersheba", 31.2518, 34.7915),
            ("Rehovot", 31.8948, 34.8110)
        };

        string[] customerNames =
        {
            "Noah Cohen", "Daniel Levy", "Maya Rosen", "David Friedman",
            "Lior Avrahami","Tamar Ben-David", "Eitan Barak", "Shira Saban", "Adam Danino",
            "Rachel Neuman", "Itay Hershkovitz", "Roni Gabay", "Gal Zahavi", "Nina Ronen",
            "Amir Ilan", "Dana Marciano", "Jonathan Tzadok", "Liad Baruch", "Omer Amir",
            "Ruth Hazan", "Eli Mor", "Sara Peretz", "Ben Avital", "Hila Levi", "Yoni Weiss",
            "Nadav Regev", "Tal Segal", "Naomi Klein", "Oren Goldstein", "Yael Mizrahi",
            "Ariel Rubin", "Noa Shahar", "Idan Azulay", "Romi Halevi", "Erez Dayan",
            "Galit Ashkenazi", "Doron Koren", "Liat Morad", "Ori Katz", "Ella Shemesh",
            "Ron Biton", "Talia Oren", "Gadi Ben-Haim", "Eden Cohen", "Noy Levi",
            "Roy Shaked", "Alon Baruch", "Michal Dahan", "Yarden Golan", "Nadav Tal"
        };

        string[] PackageDetails =
        {
            "Canvas painting - medium size", "Fragile - framed artwork", "Sculpture package - heavy",
            "Photography print envelope", "Limited edition art box"
        };

        string[] Descriptions =
        {
            "Standard art delivery", "Handle with care - original artwork", "Urgent exhibition piece - deliver directly to gallery",
            "Check authenticity certificate before handover", "Carefully packed for collector"
        };

        //לוקח את כל הערכים המוגדרים ב-Enum בשם OrderTypeוממיר אותם למערך (Array) מסוג OrderType.
        var orderTypes = Enum.GetValues(typeof(OrderType)).Cast<OrderType>().ToArray(); //להשתמש באינם עצמו?

        for (int i = 0; i < 50; i++)
        {
            var (address, latitude, longitude) = addresses[i % addresses.Length];
            var type = orderTypes[s_rand.Next(orderTypes.Length)]; //קשור לאינם
            var name = customerNames[i];
            string phone = $"05{s_rand.Next(0, 9):D1}-{s_rand.Next(100, 999):D3}-{s_rand.Next(1000, 9999):D4}"; //להכניס לפונקציה
            // להכניס לפונקציה
            DateTime clockNow = s_dalConfig!.Clock;
            int daysBack = s_rand.Next(0, 1461);
            int hoursOffset = s_rand.Next(7, 21);
            int minutesOffset = s_rand.Next(0, 60);
            int secondsOffset = s_rand.Next(0, 60);
            DateTime OpeningTime = clockNow.AddDays(-daysBack).AddHours(hoursOffset).AddMinutes(minutesOffset).AddSeconds(secondsOffset);
            //
            string details = PackageDetails[i % PackageDetails.Length];
            string descrip = Descriptions[i % Descriptions.Length];

            s_dalOrder.Create(new Order(0, type, address, latitude, longitude, name, phone, OpeningTime, details, descrip));
        }

    }

    private static void createDeliveries()
    {

    }

}
