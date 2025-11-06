namespace DalTest;
using DalApi;
using Dal;
using DO;
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
        Courier[] newCouriers = new Courier[20];
        string[] firstNames = { "Yael", "Daniel", "Omri", "Roni", "Noam", "Michal", "Eitan", "Maya", "Tamar", "Guy" };
        string[] lastNames = { "Levi", "Cohen", "Barak", "Friedman", "Avrahami", "Saban", "Danino", "Rosen", "Neuman", "Hershkovitz" };

        for (int i = 0; i < 20; i++)
        {
            int id = s_rand.Next(100000000, 999999999);
            string name = $"Courier{i + 1}";
            string phone = $"050-000-{s_rand.Next(1000, 9999):D4}";
            string email = $"courier{i + 1}";
            string password = $"Password{i + 1}!";
            bool isActive = s_rand.Next(0, 2) == 1;
            DeliveryType deliveryType = (DeliveryType)s_rand.Next(0, 4);
            DateTime startWorkTime = DateTime.Now.AddHours(s_rand.Next(-5, 6));
            double? maxDist = deliveryType != DeliveryType.ByFoot ? s_rand.Next(5, 51) : null;
           // newCouriers[i] = new Courier(id, name, phone, email, password, isActive, deliveryType, startWorkTime, maxDist);
        }

    }
    /*
     public static void InitializeCouriers(List<Courier> existingList)
        {
            Courier[] newCouriers =
            {
                new Courier(327465182, "Yael Levi", "052-412-3789", "yael.levi12@example.com", "YL!2025levi", true, DeliveryType.Vehicle, DateTime.Parse("2025-11-04T08:00:00"), 35),
                new Courier(038729416, "Daniel Cohen", "050-993-2014", "daniel.cohen03@example.com", "Dc#M0t0bike", true, DeliveryType.Motorbike, DateTime.Parse("2025-11-04T06:30:00"), 18),
                new Courier(294716305, "Omri Barak", "053-614-8821", "omri.barak07@example.com", "Omr!Bike77", true, DeliveryType.Bike, DateTime.Parse("2025-11-04T09:15:00")),
                new Courier(207639154, "Roni Friedman", "054-227-5093", "roni.friedman01@example.com", "R0niF!oot", true, DeliveryType.OnFoot, DateTime.Parse("2025-11-04T10:00:00"), 2.0),
                new Courier(056248317, "Noam Avrahami", "052-908-4470", "noam.avrahami99@example.com", "N0amA#99", false, DeliveryType.OnFoot, DateTime.Parse("2025-11-03T14:00:00")),
                new Courier(183740625, "Michal Saban", "058-116-3347", "michal.saban45@example.com", "MS!Vehicle22", true, DeliveryType.Vehicle, DateTime.Parse("2025-11-04T07:45:00"), 22),
                new Courier(098172463, "Eitan Danino", "050-441-7758", "eitan.danino14@example.com", "Et@nM0t0", true, DeliveryType.Motorbike, DateTime.Parse("2025-11-04T12:00:00"), 12),
                new Courier(234681957, "Maya Rosen", "052-773-2096", "maya.rosen66@example.com", "My@B!ke66", false, DeliveryType.Bike, DateTime.Parse("2025-10-28T08:30:00")),
                new Courier(134975806, "Tamar Neuman", "053-980-1124", "tamar.neuman20@example.com", "Tn#Foot20", true, DeliveryType.OnFoot, DateTime.Parse("2025-11-04T11:00:00"), 1.5),
                new Courier(209347681, "Guy Hershkovitz", "054-663-4412", "guy.hershkovitz05@example.com", "GH!B!ke05", true, DeliveryType.Bike, DateTime.Parse("2025-11-04T05:45:00"), 7),
                new Courier(347812905, "Alon Gabai", "052-331-9980", "alon.gabai31@example.com", "Alon#V31", false, DeliveryType.Vehicle, DateTime.Parse("2025-11-01T09:00:00")),
                new Courier(128705439, "Shira Zahavi", "050-226-5583", "shira.zahavi88@example.com", "SZ!Moto88", true, DeliveryType.Motorbike, DateTime.Parse("2025-11-04T13:20:00"), 24),
                new Courier(239861754, "Idan Rosen", "053-410-6677", "idan.rosen77@example.com", "IdR#B!ke77", true, DeliveryType.Bike, DateTime.Parse("2025-11-04T15:30:00"), 6),
                new Courier(174293865, "Noa Hasson", "058-992-0041", "noa.hasson08@example.com", "N0a#Foot08", true, DeliveryType.OnFoot, DateTime.Parse("2025-11-04T16:45:00")),
                new Courier(308621974, "Lior Ronen", "052-555-1239", "lior.ronen42@example.com", "LR!Veh42", true, DeliveryType.Vehicle, DateTime.Parse("2025-11-04T07:00:00"), 15),
                new Courier(025783469, "Rotem Ilan", "054-101-8870", "rotem.ilan26@example.com", "RoI#Moto26", false, DeliveryType.Motorbike, DateTime.Parse("2025-10-20T09:30:00")),
                new Courier(281946730, "Adi Marciano", "050-777-4433", "adi.marciano11@example.com", "AM!Bike11", true, DeliveryType.Bike, DateTime.Parse("2025-11-04T14:15:00"), 5),
                new Courier(119850342, "Yonatan Tzadok", "053-224-9005", "yonatan.tzadok55@example.com", "YT!Foot55", true, DeliveryType.OnFoot, DateTime.Parse("2025-11-04T08:30:00"), 2.5),
                new Courier(293670418, "Hila Baruch", "058-310-7722", "hila.baruch02@example.com", "HB!Veh02", true, DeliveryType.Vehicle, DateTime.Parse("2025-11-04T06:00:00"), 40),
                new Courier(036192875, "Roi Amir", "052-888-3344", "roi.amir67@example.com", "RA#Moto67", true, DeliveryType.Motorbike, DateTime.Parse("2025-11-04T17:00:00"), 30)
            };

            existingList.AddRange(newCouriers);
        }
    }
     */
    private static void createOrders()
    {
        
    }
    private static void createDeliveries()
    {
       
    }

}
