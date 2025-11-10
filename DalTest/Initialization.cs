namespace DalTest;
using DalApi;
using DO;
using System;

/// <summary>
/// a static class for initializing the DAL with test data
/// </summary>
public static class Initialization
{
    private static IDal? s_dal;

    private static readonly Random s_rand = new();

    /// <summary>
    /// generates a random phone number in the format 05X-XXX-XXXX
    /// </summary>
    /// <returns></returns>
    private static string RandomPhoneNumber() => $"05{s_rand.Next(0, 9):D1}-{s_rand.Next(100, 999):D3}-{s_rand.Next(1000, 9999):D4}";

    /// <summary>
    /// generates a random email based on the provided name
    /// </summary>
    /// <param name="Name"></param>
    /// <returns></returns>
    private static string RandomEmail(string Name) => Name.Replace(" ", ".").ToLower() + "@gmail.com";

    /// <summary>
    /// generates a random password based on the provided name
    /// </summary>
    /// <param name="Name"></param>
    /// <returns></returns>
    private static string RandomPassword(string Name) => $"{Name.Replace(" ", "#").ToLower()}{s_rand.Next(0, 21):D2}";

    /// <summary>
    /// generates a random DateTime within the last 5 years at a random hour between 7 AM and 9 PM
    /// </summary>
    /// <param name="Time"></param>
    /// <returns></returns>
    private static DateTime RandomTime(DateTime Time)
    {
        int daysBack = s_rand.Next(0, 1827);
        int hoursOffset = s_rand.Next(7, 21);
        int minutesOffset = s_rand.Next(0, 61);
        int secondsOffset = s_rand.Next(0, 61);

        return Time.AddDays(-daysBack).AddHours(hoursOffset).AddMinutes(minutesOffset).AddSeconds(secondsOffset);
    }

    /// <summary>
    /// generates a random maximum distance based on the delivery type
    /// </summary>
    /// <param name="type"></param>
    /// <param name="rand"></param>
    /// <returns></returns>
    private static double getRandomMaxDistance(DeliveryType type, Random rand)
    {
        // Define distance ranges for each delivery type
        var (min, max) = type switch
        {
            DeliveryType.Car => (50, 350),
            DeliveryType.Motorcycle => (2, 50),
            DeliveryType.Bicycle => (1, 15),
            DeliveryType.ByFoot => (1, 5),
            _ => (1, 10)
        };

        return rand.Next(min, max + 1);
    }
    /// <summary>
    /// haversine formula to calculate distance between two lat/lon points - written as a base by AI and rewritten and corrected by us
    /// </summary>
    /// <param name="lat1"></param>
    /// <param name="lon1"></param>
    /// <param name="lat2"></param>
    /// <param name="lon2"></param>
    /// <returns></returns>
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

    /// <summary>
    /// provides conversion from degrees to radians
    /// </summary>
    /// <param name="deg"></param>
    /// <returns></returns>
    private static double DegreesToRadians(double deg) => deg * (Math.PI / 180);

    /// <summary>
    /// function to pick a random courier who can handle the given distance
    /// </summary>
    /// <param name="allCouriers"></param>
    /// <param name="distKm"></param>
    /// <returns></returns>
    private static Courier? PickCourierForDistance(List<Courier> allCouriers, double distKm)
    {
        // Filter couriers who can handle the distance
        var pool = allCouriers
            .Where(c => c.MaxDist == null || c.MaxDist.Value >= distKm)
            .ToList();
        if (pool.Count == 0) return null;
        return pool[s_rand.Next(pool.Count)];
    }

    /// <summary>
    /// function to check if a new time segment overlaps with existing segments in the schedule
    /// </summary>
    /// <param name="schedule"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    private static bool HasOverlap(List<(DateTime Start, DateTime? End)> schedule, DateTime start, DateTime? end)
    {
        // Check each segment in the schedule for overlap
        foreach (var seg in schedule)
        {
            var s = seg.Start;
            var e = seg.End ?? DateTime.MaxValue;
            var newEnd = end ?? DateTime.MaxValue;
            if (start < e && s < newEnd) return true;
        }
        return false;
    }

    /// <summary>
    /// creates 20 random couriers and adds them to the DAL
    /// </summary>
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

            // Ensure unique ID
            do
                id = s_rand.Next(100000000, 999999999);
            while (s_dal!.Courier.Read(id) != null);

            string name = people[i];
            string phone = RandomPhoneNumber();
            string email = RandomEmail(name);
            string password = RandomPassword(name);
            bool isActive = s_rand.Next(0, 100) < 80; // 80% chance to be active
            DeliveryType deliveryType = (DeliveryType)s_rand.Next(0, 4); // Random delivery type
            DateTime startWorkTime = RandomTime(s_dal!.Config.Clock); // Random start work time
            double? maxDist = getRandomMaxDistance(deliveryType, s_rand); // Random max distance

            // Create and add the courier to the DAL
            s_dal!.Courier.Create(new Courier(id, name, phone, email, password, isActive, deliveryType, startWorkTime, maxDist));
        }
    }

    /// <summary>
    /// creates 50 random orders and adds them to the DAL
    /// </summary>
    private static void createOrders()
    {
        // Predefined addresses with latitudes and longitudes
        var addresses = new (string address, double latitude, double longitude)[]
        {
            ("Herzl 10, Tel Aviv", 32.0675, 34.7775), ("Jaffa Road 2, Jerusalem", 31.7780, 35.2345),
            ("Ha'arbaa 14, Herzliya", 32.1632, 34.8406), ("Begin 101, Petah Tikva", 32.0880, 34.8875),
            ("Sderot Hen 5, Haifa", 32.8040, 34.9896), ("Ha'atzmaut 25, Bat Yam", 32.0210, 34.7544),
            ("Modi'in", 31.8989, 35.0078), ("Ashdod", 31.8044, 34.6553),
            ("Beersheba", 31.2518, 34.7915), ("Rehovot", 31.8948, 34.8110)
        };

        // Predefined customer names
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

        // Predefined package details and descriptions
        string[] PackageDetails =
        {
            "Canvas painting - medium size", "Fragile - framed artwork", "Sculpture package - heavy",
            "Photography print envelope", "Limited edition art box"
        };

        // Predefined descriptions
        string[] Descriptions =
        {
            "Standard art delivery", "Handle with care - original artwork",
            "Urgent exhibition piece - deliver directly to gallery",
            "Check authenticity certificate before handover", "Carefully packed for collector"
        };

        // Create 50 orders
        for (int i = 0; i < 50; i++)
        {
            var (address, latitude, longitude) = addresses[i % addresses.Length];
            OrderType orderTypes = (OrderType)s_rand.Next(0, 3);
            var name = customerNames[i];
            string phone = RandomPhoneNumber();
            DateTime OpeningTime = RandomTime(s_dal!.Config.Clock);
            string details = PackageDetails[i % PackageDetails.Length];
            string descrip = Descriptions[i % Descriptions.Length];

            // Ensure uniqueness
            var exists = s_dal!.Order.ReadAll().Any(o => o.Address == address && o.CustomerName == name && o.CustomerPhone == phone);

            // Only create the order if it doesn't already exist
            if (!exists)
                s_dal!.Order.Create(new Order(0, orderTypes, address, latitude, longitude, name, phone, OpeningTime, details, descrip));
        }
    }

    /// <summary>
    /// creates deliveries for some of the orders in the DAL - written as a base by AI and rewritten and corrected by us
    /// </summary>
    private static void createDeliveries()
    {
        // Read all orders and couriers
        var allOrders = s_dal!.Order.ReadAll().ToList();
        var allCouriers = s_dal!.Courier.ReadAll().ToList();

        // If no orders or couriers, exit
        if (allOrders.Count == 0 || allCouriers.Count == 0)
            return;

        // Prepare data structures
        var availableOrders = new List<DO.Order>(allOrders);
        var courierSchedule = new Dictionary<int, List<(DateTime Start, DateTime? End)>>();

        // Initialize courier schedules
        foreach (var c in allCouriers)
            courierSchedule[c.Id] = new List<(DateTime, DateTime?)>();

        // Determine how many orders to mark as finished and running
        int finishedTarget = Math.Min(20, availableOrders.Count);
        int runningTarget = Math.Min(10, Math.Max(0, availableOrders.Count - finishedTarget));

        // Shuffle available orders randomly
        var shuffledOrders = availableOrders.OrderBy(_ => s_rand.Next()).ToList();

        // Select orders to be finished and running
        var finished = shuffledOrders.Take(finishedTarget).ToList();
        var running = shuffledOrders.Skip(finishedTarget).Take(runningTarget).ToList();

        DateTime now = s_dal!.Config.Clock;

        // Process finished orders
        foreach (var order in finished.ToList())
        {
            // Calculate distance from company to order location
            double dist = CalculateDistanceFromCompany(order.Latitude, order.Longitude,
                    s_dal!.Config.CompenyLatitude!.Value, s_dal!.Config.CompenyLongitude!.Value);
            var courier = PickCourierForDistance(allCouriers, dist);

            // If no suitable courier found, skip this order
            if (courier == null)
                continue;

            // Determine earliest possible start time
            DateTime earliestStart = order.OrderOpeningTime.AddMinutes(5);
            if (earliestStart < order.OrderOpeningTime) earliestStart = order.OrderOpeningTime;
            if (earliestStart > now) earliestStart = now;

            // Try to place the delivery in the courier's schedule
            DateTime start = earliestStart;
            bool placed = false;

            // Attempt up to 50 times to find a non-overlapping time slot
            for (int attempt = 0; attempt < 50; attempt++)
            {
                // Randomly select a start time within the allowed window
                int maxMinutesWindow = (int)Math.Max(1, (now - earliestStart).TotalMinutes);
                int addMinutes = (maxMinutesWindow == 0) ? 0 : s_rand.Next(0, maxMinutesWindow);
                start = earliestStart.AddMinutes(addMinutes);

                // Randomly determine a delivery duration between 20 and 180 minutes
                int durationMinutes = s_rand.Next(20, 180);
                DateTime end = start.AddMinutes(durationMinutes);

                // Ensure the end time does not exceed the current time
                if (end > now) end = start.AddMinutes(s_rand.Next(10, Math.Min(60, durationMinutes)));

                // Check for schedule overlap
                if (!HasOverlap(courierSchedule[courier.Id], start, end))
                {
                    // No overlap found, schedule the delivery
                    courierSchedule[courier.Id].Add((start, end));

                    // Randomly select a closed status for the order
                    var closedStatuses = new[]
                    {
                    OrderStatus.Delivered,
                    OrderStatus.Refused,
                    OrderStatus.Cancelled,
                    OrderStatus.InviterNotFound,
                    OrderStatus.Failed
                    };

                    // Pick a random closed status
                    var endStatus = closedStatuses[s_rand.Next(closedStatuses.Length)];

                    // Create the delivery record
                    s_dal!.Delivery.Create(new DO.Delivery(0, order.Id, courier.Id, order.TypeOfOrder, start, dist, endStatus, end));
                    availableOrders.RemoveAll(o => o.Id == order.Id);

                    // Mark as placed and exit the loop
                    placed = true;
                    break;
                }
            }

            // If not placed after all attempts, continue to the next order
            if (!placed)
                continue;
        }

        // Process running orders
        foreach (var order in running.ToList())
        {
            // Calculate distance from company to order location
            double dist = CalculateDistanceFromCompany(order.Latitude, order.Longitude,
                    s_dal!.Config.CompenyLatitude!.Value, s_dal!.Config.CompenyLongitude!.Value);

            var courier = PickCourierForDistance(allCouriers, dist);

            // If no suitable courier found, skip this order
            if (courier == null)
                continue;

            // Determine earliest possible start time
            DateTime earliestStart = order.OrderOpeningTime.AddMinutes(5);

            // Ensure earliest start is not before order opening time
            if (earliestStart > now) earliestStart = now;

            // Try to place the delivery in the courier's schedule
            DateTime start = earliestStart;
            bool placed = false;

            // Attempt up to 50 times to find a non-overlapping time slot
            for (int attempt = 0; attempt < 50; attempt++)
            {
                // Randomly select a start time within the allowed window
                int maxMinutesWindow = (int)Math.Max(1, (now - earliestStart).TotalMinutes);
                int addMinutes = (maxMinutesWindow == 0) ? 0 : s_rand.Next(0, maxMinutesWindow);
                start = earliestStart.AddMinutes(addMinutes);
                DateTime? end = null;

                // Check for schedule overlap
                if (!HasOverlap(courierSchedule[courier.Id], start, end))
                {
                    courierSchedule[courier.Id].Add((start, null));

                    // Create the delivery record
                    s_dal!.Delivery.Create(new DO.Delivery(0, order.Id, courier.Id, order.TypeOfOrder, start, dist, null, null));

                    availableOrders.RemoveAll(o => o.Id == order.Id);
                    placed = true;
                    break;
                }
            }

            // If not placed after all attempts, continue to the next order
            if (!placed)
                continue;
        }
    }

    /// <summary>
    /// initializes the DAL by resetting config and lists, and creating test data
    /// </summary>
    /// <param name="dalConfig"></param>
    /// <param name="dalCourier"></param>
    /// <param name="dalOrder"></param>
    /// <param name="dalDelivery"></param>
    /// <exception cref="NullReferenceException"></exception>
    public static void Do(IDal dal)
    {
        // Assign DAL interfaces, throwing exceptions if any are null
        s_dal = dal ?? throw new NullReferenceException("DAL object can not be null!");

        Console.WriteLine("Reset Configuration values and List values...");
        s_dal.ResetDB();

        Console.WriteLine("Initializing Couriers...");
        createCouriers();

        Console.WriteLine("Initializing Orders...");
        createOrders();

        Console.WriteLine("Initializing Deliveries...");
        createDeliveries();

        Console.WriteLine("Initialization done.");
    }
}