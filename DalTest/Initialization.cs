using DalApi;
using DO;
using Dal;

namespace DalTest;

public static class Initialization
{
    private static ICourier? s_dalCourier;
    private static IOrder? s_dalOrder;
    private static IDelivery? s_dalDelivery;
    private static IConfig? s_dalConfig;

    private static readonly Random s_rand = new();

    private static string RandomPhoneNumber() => $"05{s_rand.Next(0, 9):D1}-{s_rand.Next(100, 999):D3}-{s_rand.Next(1000, 9999):D4}";
    private static string RandomEmail(string Name) => Name.Replace(" ", ".").ToLower() + "@gmail.com";
    private static string RandomPassword(string Name) => $"{Name.Replace(" ", "#").ToLower()}{s_rand.Next(0, 21):D2}";
    private static DateTime RandomTime(DateTime Time)
    {
        int daysBack = s_rand.Next(0, 1827);
        int hoursOffset = s_rand.Next(7, 21);
        int minutesOffset = s_rand.Next(0, 61);
        int secondsOffset = s_rand.Next(0, 61);
        return Time.AddDays(-daysBack).AddHours(hoursOffset).AddMinutes(minutesOffset).AddSeconds(secondsOffset);
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

    private static double CalculateDistanceFromCompany(double lat1, double lon1, double lat2, double lon2)
    {
        double R = 6371;
        double dLat = DegreesToRadians(lat2 - lat1);
        double dLon = DegreesToRadians(lon2 - lon1);
        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    private static double DegreesToRadians(double deg) => deg * (Math.PI / 180);
    private static double CalculateDistanceFromCompany(double latitude, double longitude, object value1, object value2)
    {
        throw new NotImplementedException();
    }
    private static void createCouriers()
    {
        string[] people =
        {
            "Noa Levi","Daniel Cohen","Yael Barak","Roi Avrahami","Maya Friedman","Omri Danino",
            "Tamar Rosen","Eitan Gabai","Shira Neuman","Guy Hershkovitz","Alon Zahavi","Hila Ronen","Idan Marciano",
            "Rotem Tzadok","Adi Baruch","Yonatan Amir","Michal Saban","Lior Gross","Roni Hasson","Noam Ben-David"
        };

        for (int i = 0; i < 20; i++)
        {
            int id;
            do
                id = s_rand.Next(100000000, 999999999);
            while (s_dalCourier!.Read(id) != null);

            string name = people[i];
            string phone = RandomPhoneNumber();
            string email = RandomEmail(name);
            string password = RandomPassword(name);
            bool isActive = s_rand.Next(0, 100) < 80;
            DeliveryType deliveryType = (DeliveryType)s_rand.Next(0, 4);
            DateTime startWorkTime = RandomTime(s_dalConfig!.Clock);
            double? maxDist = getRandomMaxDistance(deliveryType, s_rand);

            s_dalCourier.Create(new Courier(id, name, phone, email, password, isActive, deliveryType, startWorkTime, maxDist));
        }
    }

    private static void createOrders()
    {
        var addresses = new (string address, double latitude, double longitude)[]
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
            "Standard art delivery", "Handle with care - original artwork",
            "Urgent exhibition piece - deliver directly to gallery",
            "Check authenticity certificate before handover", "Carefully packed for collector"
        };

        for (int i = 0; i < 50; i++)
        {
            var (address, latitude, longitude) = addresses[i % addresses.Length];
            OrderType orderTypes = (OrderType)s_rand.Next(0, 3);
            var name = customerNames[i];
            string phone = RandomPhoneNumber();
            DateTime OpeningTime = RandomTime(s_dalConfig!.Clock);
            string details = PackageDetails[i % PackageDetails.Length];
            string descrip = Descriptions[i % Descriptions.Length];

            s_dalOrder!.Create(new Order(0, orderTypes, address, latitude, longitude, name, phone, OpeningTime, details, descrip));
        }
    }

    private static void createDeliveries()
    {
        var couriers = s_dalCourier!.ReadAll().Where(c => c.IsActive).ToList();
        var orders = s_dalOrder!.ReadAll().ToList();

        foreach (var order in orders)
        {

            double distance = 0;
            if (s_dalConfig!.CompenyLatitude is not null && s_dalConfig!.CompenyLongitude is not null)
            {
                distance = CalculateDistanceFromCompany(order.Latitude, order.Longitude,
                                                        s_dalConfig.CompenyLatitude.Value, s_dalConfig.CompenyLongitude.Value);
            }

            var possibleCouriers = couriers
                .Where(c => !c.MaxDist.HasValue || c.MaxDist.Value >= distance)
                .ToList();

            if (!possibleCouriers.Any())
                continue;

            var courier = possibleCouriers[s_rand.Next(possibleCouriers.Count)];

            DateTime startTime = order.OrderOpeningTime.AddHours(s_rand.Next(1, 48));
            bool finished = s_rand.Next(0, 100) < 70;

            DateTime? endTime = finished ? startTime.AddHours(s_rand.Next(1, 12)) : null;
            OrderStatus? endStatus = finished ? (OrderStatus?)s_rand.Next(Enum.GetValues(typeof(OrderStatus)).Length) : null;

            double actualDistance = distance + s_rand.NextDouble() * 0.5;

            s_dalDelivery!.Create(new(0, order.Id, courier.Id, order.TypeOfOrder, startTime, actualDistance, endStatus, endTime));
        }
    }
}