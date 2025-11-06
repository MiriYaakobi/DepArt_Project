namespace DalTest;
using Dal;
using DalApi;
using DO;
using System.Net;
using System.Numerics;
using System.Xml.Linq;


public static class Initialization
{
    private static ICourier? s_dalCourier;
    private static IOrder? s_dalOrder;
    private static IDelivery? s_dalDelivery;
    private static IConfig? s_dalConfig;

    private static readonly Random s_rand = new();

    private static void createCouriers()
    {
        string[] people =
        {"Noa Levi","Daniel Cohen","Yael Barak","Roi Avrahami","Maya Friedman","Omri Danino",
        "Tamar Rosen","Eitan Gabai","Shira Neuman","Guy Hershkovitz","Alon Zahavi","Hila Ronen","Idan Marciano",
        "Rotem Tzadok","Adi Baruch","Yonatan Amir","Michal Saban","Lior Gross","Roni Hasson","Noam Ben-David"
        };

        char[] chars = { '!', '@', '#', '$', '%', '^', '&', '*'};

        for (int i = 0; i < 20; i++)
        {
            int id;
            do
                id = s_rand.Next(100000000, 999999999);
            while (s_dalCourier!.Read(id) != null);

            string name = people[i];
            string phone = $"05{s_rand.Next(0, 9):D1}-{s_rand.Next(100, 999):D3}-{s_rand.Next(1000, 9999):D4}";
            string email = $"{people[i]}@gmail.com";
            string password = $"{people[i]}{s_rand.Next(100, 999):D3}{chars[i] % chars.Length}";
            bool isActive = s_rand.Next(0, 100) < 80;
            DeliveryType deliveryType = (DeliveryType)s_rand.Next(0, 3);

            DateTime clockNow = s_dalConfig!.Clock;
            int daysBack = s_rand.Next(0, 1461);
            int hoursOffset = s_rand.Next(7, 21);//
            int minutesOffset = s_rand.Next(0, 60);
            int secondsOffset = s_rand.Next(0, 60);
            DateTime startWorkTime = clockNow.AddDays(-daysBack).AddHours(hoursOffset).AddMinutes(minutesOffset).AddSeconds(secondsOffset);

            double? maxDist = getRandomMaxDistance(deliveryType, s_rand);
            s_dalCourier.Create(new Courier(id, name, phone, email, password, isActive, deliveryType, startWorkTime, maxDist));
        }
    }
    private static void createOrders()
    {

 
    }
    private static void createDeliveries()
    {

    }
    private static double getRandomMaxDistance(DeliveryType type, Random rand)
    {
        int min, max;
        switch (type)
        {
            case DeliveryType.Car:
                min = 50; max = 350; break;
            case DeliveryType.Motorcycle:
                min = 2; max = 50; break;
            case DeliveryType.Bicycle:
                min = 1; max = 15; break;
            case DeliveryType.ByFoot:
                min = 1; max = 5; break;
            default:
                min = 1; max = 10; break;
        }
        return rand.Next(min, max + 1);
    }
}

