using BLApi;

namespace BlTest;

internal class Program
{
    static readonly IBl s_bl = Factory.Get();
    private enum MainMenuOptions
    {
        Exit,
        Admin,
        Courier,
        Order
    }

    private enum CurierMenuOptions
    {
        Exit,
        Login,
        Create,
        Read,
        ReadAll,
        Update,
        Delete
    }

    private enum OrderMenuOptions
    {
        Exit,
        Create,
        Read,
        ReadAll,
        Update,
        Delete,
        Cancel,
        OrderSummary,
        CompleteDelivery,
        ChooseOrder,
        ClosedDeliveries,
        ReadAllOpenOrders
    }

    private enum AdminMenuOptions
    {
        Exit,
        Reset,
        Initialize,
        GetClock,
        ForwardClock,
        GetConfig,
        SetConfig
    }

    private static int AdminID = 123456782; // Default Admin ID for testing

    private static MainMenuOptions ShowMainMenu()
    {
        Console.WriteLine("\n--- MAIN MENU ---");
        Console.WriteLine("0: Exit");
        Console.WriteLine("1: Admin Management");
        Console.WriteLine("2: Courier Management");
        Console.WriteLine("3: Order Management");
        Console.Write("Enter your choice: ");

        int choice;
        while (!int.TryParse(Console.ReadLine(), out choice) || !Enum.IsDefined(typeof(MainMenuOptions), choice))
        {
            Console.Write("Invalid input. Please enter a number between 0 and 3: ");
        }
        return (MainMenuOptions)choice;
    }

    private static CurierMenuOptions ShowCurierdMenu()
    {
        Console.WriteLine($"\n--- CURIER MENU ---");
        Console.WriteLine("0: Exit"); // Exit
        Console.WriteLine("1: Login");
        Console.WriteLine("2: Create");
        Console.WriteLine("3: Read");
        Console.WriteLine("4: ReadAll");
        Console.WriteLine("5: Update");
        Console.WriteLine("6: Delete");
        Console.Write("Enter your choice: ");

        int choice;

        // Validate input
        while (!int.TryParse(Console.ReadLine(), out choice) || !Enum.IsDefined(typeof(CurierMenuOptions), choice))
        {
            Console.Write("Invalid input. Please enter a number between 0 and 5: ");
        }
        return (CurierMenuOptions)choice;
    }

    private static OrderMenuOptions ShowOrderMenu()
    {
        Console.WriteLine($"\n--- ORDER MENU ---");
        Console.WriteLine("0: Exit"); // Exit
        Console.WriteLine("1: Create");
        Console.WriteLine("2: Read");
        Console.WriteLine("3: ReadAll");
        Console.WriteLine("4: Update");
        Console.WriteLine("5: Delete");
        Console.WriteLine("6: Cancel");
        Console.WriteLine("7: Orders Summary");
        Console.WriteLine("8: Watch Completed Deliveries");
        Console.WriteLine("9: Choose An Order For treatment");
        Console.WriteLine("10: Watch Closed Deliveries");
        Console.WriteLine("11: Read All Open Orders");
        Console.Write("Enter your choice: ");

        int choice;

        // Validate input
        while (!int.TryParse(Console.ReadLine(), out choice) || !Enum.IsDefined(typeof(OrderMenuOptions), choice))
        {
            Console.Write("Invalid input. Please enter a number between 0 and 11: ");
        }
        return (OrderMenuOptions)choice;
    }
    private static void PrintException(BO.BlException ex)
    {
        Console.WriteLine($"\nERROR: {ex.GetType().Name}");
        Console.WriteLine(ex.Message);
        if (ex.InnerException != null)
        {
            Console.WriteLine($"{ex.InnerException.GetType().Name} - {ex.InnerException.Message}");
        }
        Console.ResetColor();
    }
    private static void LoginCourier()
    {
        Console.WriteLine("\n--- Courier Login ---");
        Console.Write("Enter Courier ID: ");
        if (!int.TryParse(Console.ReadLine(), out int id)) { Console.WriteLine("Invalid ID format."); return; }
        Console.Write("Enter Password: ");
        string? password = Console.ReadLine();
        try
        {
            BO.UserRole role = s_bl.Courier.Login(id, password!);
            if (role != BO.UserRole.None)
            {
                Console.WriteLine($"\n Login successful. Welcome!");
            }
            else
            {
                Console.WriteLine("\n Login failed. Invalid ID or password.");
            }
        }
        catch (BO.BlNotAuthorizedException ex) { PrintException(ex); }
    }
    private static void AddCourier()
    {
        Console.WriteLine("\n--- Add New Courier ---");

        Console.Write("Enter Courier ID (9 digits): ");
        if (!int.TryParse(Console.ReadLine(), out int id)) { Console.WriteLine("Invalid ID format."); return; }

        Console.Write("Enter Full Name: ");
        string? name = Console.ReadLine();

        Console.Write("Enter Phone Number: ");
        string? phone = Console.ReadLine();

        Console.Write("Enter Email: ");
        string? email = Console.ReadLine();

        Console.Write("Enter Password: ");
        string? password = Console.ReadLine();

        Console.Write("Enter Delivery Type (Car, Motorcycle, Bicycle, ByFoot): ");
        if (!Enum.TryParse(Console.ReadLine(), true, out BO.DeliveryType typeOfDelivery)) { Console.WriteLine("Invalid delivery type."); return; }

        Console.Write("Enter Max Delivery Distance: ");
        string? maxDistInput = Console.ReadLine();
        double? maxDist = null;
        if (!string.IsNullOrEmpty(maxDistInput) && double.TryParse(maxDistInput, out double tempDistance))
        {
            maxDist = tempDistance;
        }

        try
        {
            BO.Courier newCourier = new BO.Courier
            {
                Id = id,
                Name = name!,
                Phone = phone!,
                Email = email!,
                Password = password!,
                IsActive = true,
                TypeOfDelivery = typeOfDelivery,
                MaxDistance = maxDist,
                TotalOnTimeDeliveries = 0,
                TotalLateDeliveries = 0,
                CurrentOrder = null
            };
            s_bl.Courier.Create(AdminID, newCourier);

            Console.WriteLine($"\n Successfully added Courier {id} - {name}");
        }
        catch (BO.BlInvalidDataException ex) { PrintException(ex); } //validation failed
        catch (BO.BlAlreadyExistsException ex) { PrintException(ex); } //already exists
        catch (BO.BlNotAuthorizedException ex) { PrintException(ex); } //alout unauthorized
    }
    private static void GetCourier()
    {
        Console.WriteLine("\n--- Read Courier ---");
        Console.Write("Enter Courier ID to get: ");
        if (!int.TryParse(Console.ReadLine(), out int id)) { Console.WriteLine("Invalid ID format."); return; }

        try
        {
            BO.Courier? courier = s_bl.Courier.Read(AdminID, id);

            if (courier != null)
            {
                Console.WriteLine($"\n Courier Details:\n{courier}");
            }
        }
        catch (BO.BlDoesNotExistException ex) { PrintException(ex); }
        catch (BO.BlNotAuthorizedException ex) { PrintException(ex); }
    }
    private static void ListAllCouriers()
    {
        Console.WriteLine("\n--- List All Couriers ---");

        try
        {
            IEnumerable<BO.CourierInList> couriers = s_bl.Courier.ReadAll(AdminID);

            if (!couriers.Any())
            {
                Console.WriteLine("No couriers found.");
                return;
            }

            foreach (var courier in couriers)
            {
                Console.WriteLine(courier);
            }
        }
        catch (BO.BlNotAuthorizedException ex) { PrintException(ex); }
    }
    private static void UpdateCourier()
    {
        Console.WriteLine("\n--- Update Courier ---");
        Console.Write("Enter Courier ID to update: ");
        if (!int.TryParse(Console.ReadLine(), out int id)) { Console.WriteLine("Invalid ID format."); return; }

        try
        {
            BO.Courier oldCourier = s_bl.Courier.Read(AdminID, id)!;

            Console.WriteLine($"\nUpdating Courier {id}. Current Name: {oldCourier.Name}");

            Console.Write("Enter new Name (Enter to skip): ");
            string? newName = Console.ReadLine();

            Console.Write("Enter new Phone (Enter to skip): ");
            string? newPhone = Console.ReadLine();

            Console.Write("Enter new Password (Enter to skip): ");
            string? newPassword = Console.ReadLine();

            BO.Courier updatedCourier = new BO.Courier
            {
                Id = oldCourier.Id,
                Name = string.IsNullOrEmpty(newName) ? oldCourier.Name : newName!,
                Phone = string.IsNullOrEmpty(newPhone) ? oldCourier.Phone : newPhone!,
                Email = oldCourier.Email,
                Password = string.IsNullOrEmpty(newPassword) ? oldCourier.Password : newPassword!,
                IsActive = oldCourier.IsActive,
                TypeOfDelivery = oldCourier.TypeOfDelivery,
                MaxDistance = oldCourier.MaxDistance,
                TotalOnTimeDeliveries = oldCourier.TotalOnTimeDeliveries,
                TotalLateDeliveries = oldCourier.TotalLateDeliveries,
                CurrentOrder = oldCourier.CurrentOrder
            };

            s_bl.Courier.Update(AdminID, updatedCourier);

            Console.WriteLine($"\n Successfully updated Courier {id}");
        }
        catch (BO.BlDoesNotExistException ex) { PrintException(ex); }
        catch (BO.BlInvalidDataException ex) { PrintException(ex); }
        catch (BO.BlNotAuthorizedException ex) { PrintException(ex); }
    }
    private static void DeleteCourier()
    {
        Console.WriteLine("\n--- Delete Courier ---");
        Console.Write("Enter Courier ID to delete: ");
        if (!int.TryParse(Console.ReadLine(), out int id)) { Console.WriteLine("Invalid ID format."); return; }

        try
        {
            s_bl.Courier.Delete(AdminID, id);

            Console.WriteLine($"\n Successfully deleted Courier {id}");
        }
        catch (BO.BlDoesNotExistException ex) { PrintException(ex); }
        catch (BO.BlCannotDeleteException ex) { PrintException(ex); }
        catch (BO.BlNotAuthorizedException ex) { PrintException(ex); }
    }
    private static void CourierMenu()
    {
        bool exit = false;
        while (!exit)
        {
            CurierMenuOptions choice = ShowCurierdMenu();

            try
            {
                switch (choice)
                {
                    case CurierMenuOptions.Exit: exit = true; break;
                    case CurierMenuOptions.Login: LoginCourier(); break;
                    case CurierMenuOptions.Create: AddCourier(); break;
                    case CurierMenuOptions.Read: GetCourier(); break;
                    case CurierMenuOptions.ReadAll: ListAllCouriers(); break;
                    case CurierMenuOptions.Update: UpdateCourier(); break;
                    case CurierMenuOptions.Delete: DeleteCourier(); break;
                }
            }
            catch (BO.BlException ex) { PrintException(ex); }
            catch (Exception ex) { Console.WriteLine($"An unexpected system error occurred: {ex.Message}"); }
        }
    }
    private static void AddOrder()
    {
        Console.WriteLine("\n--- Add New Order ---");

        Console.Write("Enter Order Type (Regular, Express, SameDay): ");
        if (!Enum.TryParse(Console.ReadLine(), true, out BO.OrderType typeOfOrder)) { Console.WriteLine("Invalid order type."); return; }

        Console.Write("Enter Address: ");
        string? address = Console.ReadLine();

        Console.Write("Enter Customer Name: ");
        string? customerName = Console.ReadLine();

        Console.Write("Enter Customer Phone: ");
        string? customerPhone = Console.ReadLine();

        Console.Write("Enter Description (optional, press Enter to skip): ");
        string? description = Console.ReadLine();

        BO.Order newOrder = new BO.Order
        {
            TypeOfOrder = typeOfOrder,
            Address = address!,
            CustomerName = customerName!,
            CustomerPhone = customerPhone!,
            Description = description
        };

        try
        {
            s_bl.Order.Create(AdminID, newOrder);

            Console.WriteLine($"\n Successfully requested");
        }
        catch (BO.BlInvalidDataException ex) { PrintException(ex); } //validation failed
        catch (BO.BlInvalidOperationException ex) { PrintException(ex); } //ID or Geocoding failed
        catch (BO.BlNotAuthorizedException ex) { PrintException(ex); } //alout unauthorized
    }
    private static void GetOrder()
    {
        Console.WriteLine("\n--- Read Order ---");
        Console.Write("Enter Order ID to get: ");
        if (!int.TryParse(Console.ReadLine(), out int id)) { Console.WriteLine("Invalid ID format."); return; }

        try
        {
            BO.Order order = s_bl.Order.Read(AdminID, id);

            Console.WriteLine($"\n Order Details:\n{order}"); 
        }
        catch (BO.BlDoesNotExistException ex) { PrintException(ex); }
        catch (BO.BlNotAuthorizedException ex) { PrintException(ex); }
    }
    private static void ListAllOrders()
    {
        Console.WriteLine("\n--- List All Orders ---");

        try
        {
            IEnumerable<BO.OrderInList> orders = s_bl.Order.ReadAll(AdminID);

            if (!orders.Any())
            {
                Console.WriteLine("No orders found.");
                return;
            }

            foreach (var order in orders)
            {
                Console.WriteLine(order);
            }
        }
        catch (BO.BlNotAuthorizedException ex) { PrintException(ex); }
    }
    private static void UpdateOrder()
    {
        Console.WriteLine("\n--- Update Order ---");
        Console.Write("Enter Order ID to update: ");
        if (!int.TryParse(Console.ReadLine(), out int id)) { Console.WriteLine("Invalid ID format."); return; }

        try
        {
            BO.Order oldOrder = s_bl.Order.Read(AdminID, id)!;

            Console.WriteLine($"\nUpdating Order {id}. Current Status: {oldOrder.StatusOfOrder}");

            Console.Write($"Enter new Address (current: {oldOrder.Address}) (Enter to skip): ");
            string? newAddress = Console.ReadLine();

            Console.Write($"Enter new Customer Phone (current: {oldOrder.CustomerPhone}) (Enter to skip): ");
            string? newPhone = Console.ReadLine();

            BO.Order updatedOrder = new BO.Order
            {
                Id = oldOrder.Id,
                TypeOfOrder = oldOrder.TypeOfOrder,
                Description = oldOrder.Description,
                Address = string.IsNullOrEmpty(newAddress) ? oldOrder.Address : newAddress!,
                Latitude = oldOrder.Latitude,
                Longitude = oldOrder.Longitude,
                AirDistance = oldOrder.AirDistance,
                CustomerName = oldOrder.CustomerName,
                CustomerPhone = string.IsNullOrEmpty(newPhone) ? oldOrder.CustomerPhone : newPhone!,
                PackageDetails = oldOrder.PackageDetails,
                OrderOpeningTime = oldOrder.OrderOpeningTime,
                ExpectedDeliveryTime = oldOrder.ExpectedDeliveryTime,
                MaxDeliveryTime = oldOrder.MaxDeliveryTime,
                StatusOfOrder = oldOrder.StatusOfOrder,
                TimeLinessStatus = oldOrder.TimeLinessStatus,
                RemainingDeliveryTime = oldOrder.RemainingDeliveryTime,
                DeliveryList = oldOrder.DeliveryList
            };

            s_bl.Order.Update(AdminID, updatedOrder);

            Console.WriteLine($"\n Successfully updated Order {id}");
        }
        catch (BO.BlDoesNotExistException ex) { PrintException(ex); }
        catch (BO.BlInvalidDataException ex) { PrintException(ex); }
        catch (BO.BlInvalidOperationException ex) { PrintException(ex); }
        catch (BO.BlNotAuthorizedException ex) { PrintException(ex); }
    }
    private static void DeleteOrder()
    {
        Console.WriteLine("\n--- Delete Order ---");
        Console.Write("Enter Order ID to delete: ");
        if (!int.TryParse(Console.ReadLine(), out int id)) { Console.WriteLine("Invalid ID format."); return; }

        try
        {
            s_bl.Order.Delete(AdminID, id);

            Console.WriteLine($"\nSuccessfully deleted Order {id}");
        }
        catch (BO.BlDoesNotExistException ex) { PrintException(ex); }
        catch (BO.BlInvalidOperationException ex) { PrintException(ex); }
        catch (BO.BlNotAuthorizedException ex) { PrintException(ex); }
    }
    private static void CancelOrderOperation()
    {
        Console.WriteLine("\n--- Cancel Order ---");
        Console.Write("Enter Requesting User ID (Admin/Courier performing the action): ");
        if (!int.TryParse(Console.ReadLine(), out int requestingUserId)) { Console.WriteLine("Invalid User ID format."); return; }
        Console.Write("Enter Order ID to cancel: ");
        if (!int.TryParse(Console.ReadLine(), out int orderId)) { Console.WriteLine("Invalid Order ID format."); return; }
        try
        {
            s_bl.Order.Cancel(requestingUserId, orderId);
            Console.WriteLine($"\n Order {orderId} successfully canceled.");
        }
        catch (BO.BlNotAuthorizedException ex) { PrintException(ex); }
        catch (BO.BlDoesNotExistException ex) { PrintException(ex); }
        catch (BO.BlInvalidOperationException ex) { PrintException(ex); }
        catch (BO.BlException ex) { PrintException(ex); }
    }
    private static void OrderSummaryOperation()
    {
        Console.WriteLine("\n--- Get Order Summary ---");

        Console.Write("Enter Requesting User ID: ");
        if (!int.TryParse(Console.ReadLine(), out int requestingUserId)) { Console.WriteLine("Invalid User ID format."); return; }

        try
        {
            int[] summary = s_bl.Order.GetOrderSummaryQuantities(requestingUserId);

            Console.WriteLine($"\n Orders Summary:");
            for (int i = 0; i < summary.Length; i++)
                Console.WriteLine($"{(BO.OrderStatus)i}: {summary[i]}");

        }
        catch (BO.BlDoesNotExistException ex) { PrintException(ex); }
        catch (BO.BlNotAuthorizedException ex) { PrintException(ex); }
        catch (BO.BlException ex) { PrintException(ex); }
    }
    private static void CompleteDeliveryOperation()
    {
        Console.WriteLine("\n--- Report Delivery Completion ---");

        Console.Write("Enter Courier ID reporting completion: ");
        if (!int.TryParse(Console.ReadLine(), out int courierId)) { Console.WriteLine("Invalid Courier ID format."); return; }

        Console.Write("Enter Order ID that was delivered: ");
        if (!int.TryParse(Console.ReadLine(), out int orderId)) { Console.WriteLine("Invalid Order ID format."); return; }

        Console.Write("Enter Delivery End Latitude (where delivery was finished): ");
        if (!double.TryParse(Console.ReadLine(), out double endLat)) { Console.WriteLine("Invalid Latitude format."); return; }

        Console.Write("Enter Delivery End Longitude (where delivery was finished): ");
        if (!double.TryParse(Console.ReadLine(), out double endLon)) { Console.WriteLine("Invalid Longitude format."); return; }

        try
        {
            s_bl.Order.CompleteDelivery(AdminID, courierId, orderId, endLat, endLon);

            Console.WriteLine($"\n Order {orderId} successfully marked as Delivered by Courier {courierId}.");
        }
        catch (BO.BlNotAuthorizedException ex) { PrintException(ex); }
        catch (BO.BlDoesNotExistException ex) { PrintException(ex); }
        catch (BO.BlInvalidOperationException ex) { PrintException(ex); }
        catch (BO.BlException ex) { PrintException(ex); }
    }
    private static void ChooseOrderOperation()
    {
        Console.WriteLine("\n--- Choose Order for Delivery ---");

        Console.Write("Enter Requesting User ID (Admin/Courier performing the action): ");
        if (!int.TryParse(Console.ReadLine(), out int requestingUserId)) { Console.WriteLine("Invalid User ID format."); return; }

        Console.Write("Enter Courier ID who will perform the delivery: ");
        if (!int.TryParse(Console.ReadLine(), out int courierId)) { Console.WriteLine("Invalid Courier ID format."); return; }

        Console.Write("Enter Order ID to assign: ");
        if (!int.TryParse(Console.ReadLine(), out int orderId)) { Console.WriteLine("Invalid Order ID format."); return; }

        try
        {
            s_bl.Order.ChooseOrder(requestingUserId, courierId, orderId);

            Console.WriteLine($"\n Order {orderId} successfully assigned to Courier {courierId}. Delivery process initiated.");
        }
        catch (BO.BlNotAuthorizedException ex) { PrintException(ex); }
        catch (BO.BlDoesNotExistException ex) { PrintException(ex); }
        catch (BO.BlInvalidOperationException ex) { PrintException(ex); }
        catch (BO.BlException ex) { PrintException(ex); }
    }
    private static void GetClosedDeliveriesForCourierOperation()
    {
        Console.WriteLine("\n--- View Closed Deliveries History ---");

        Console.Write("Enter Requesting User ID (Admin or Courier): ");
        if (!int.TryParse(Console.ReadLine(), out int requestingUserId)) { Console.WriteLine("Invalid User ID format."); return; }

        Console.Write("Enter Target Courier ID to view history for: ");
        if (!int.TryParse(Console.ReadLine(), out int courierId)) { Console.WriteLine("Invalid Courier ID format."); return; }

        try
        {
            IEnumerable<BO.ClosedDeliveryInList> closedDeliveries = s_bl.Order.GetClosedDeliveriesForCourier(requestingUserId, courierId);

            if (!closedDeliveries.Any())
            {
                Console.WriteLine($"\nCourier {courierId} has no completed delivery history.");
                return;
            }

            Console.WriteLine($"\n Closed Deliveries for Courier {courierId}:");
            foreach (var delivery in closedDeliveries)
            {
                Console.WriteLine(delivery);
            }
        }
        catch (BO.BlNotAuthorizedException ex) { PrintException(ex); }
        catch (BO.BlException ex) { PrintException(ex); }
    }
    private static void ListAllOpenOrders()
    {
        Console.WriteLine("\n--- List All Open Orders for Courier ---");

        Console.Write("Enter Requesting User ID (Admin or Courier): ");
        if (!int.TryParse(Console.ReadLine(), out int requestingUserId)) { Console.WriteLine("Invalid User ID format."); return; }
        
        Console.Write("Enter Courier ID to view open orders for: ");
        if (!int.TryParse(Console.ReadLine(), out int courierId)) { Console.WriteLine("Invalid Courier ID format."); return; }
        
        try
        {
            IEnumerable<BO.OpenOrderInList> openOrders = s_bl.Order.ReadAllOpenOrders(requestingUserId, courierId);
            if (!openOrders.Any())
            {
                Console.WriteLine($"\nCourier {courierId} has no open orders.");
                return;
            }
            Console.WriteLine($"\n Open Orders for Courier {courierId}:");
            foreach (var order in openOrders)
            {
                Console.WriteLine(order);
            }
        }
        catch (BO.BlNotAuthorizedException ex) { PrintException(ex); }
        catch (BO.BlException ex) { PrintException(ex); }
    }
    private static void OrderMenu()
    {
        bool exit = false;
        while (!exit)
        {
            OrderMenuOptions choice = ShowOrderMenu();

            try
            {
                switch (choice)
                {
                    case OrderMenuOptions.Exit: exit = true; break;
                    case OrderMenuOptions.Create: AddOrder(); break;
                    case OrderMenuOptions.Read: GetOrder(); break;
                    case OrderMenuOptions.ReadAll: ListAllOrders(); break;
                    case OrderMenuOptions.Update: UpdateOrder(); break;
                    case OrderMenuOptions.Delete: DeleteOrder(); break;
                    case OrderMenuOptions.Cancel: CancelOrderOperation(); break;
                    case OrderMenuOptions.OrderSummary: OrderSummaryOperation(); break;
                    case OrderMenuOptions.CompleteDelivery: CompleteDeliveryOperation(); break;
                    case OrderMenuOptions.ChooseOrder: ChooseOrderOperation(); break;
                    case OrderMenuOptions.ClosedDeliveries: GetClosedDeliveriesForCourierOperation(); break;
                    case OrderMenuOptions.ReadAllOpenOrders: ListAllOpenOrders(); break;
                }
            }
            catch (BO.BlException ex) { PrintException(ex); }
            catch (Exception ex) { Console.WriteLine($"An unexpected system error occurred: {ex.Message}"); }
        }
    }
    private static void AdvanceClockOperation()
    {
        Console.WriteLine("\n--- Advance Simulated Clock ---");

        Console.Write("Enter number of minutes to advance: ");
        if (!int.TryParse(Console.ReadLine(), out int minutes) || minutes < 0)
        {
            Console.WriteLine("Invalid minutes.");
            return;
        }

        Console.Write("Enter number of hours to advance: ");
        if (!int.TryParse(Console.ReadLine(), out int hours) || hours < 0)
        {
            Console.WriteLine("Invalid hours.");
            return;
        }

        Console.Write("Enter number of days to advance: ");
        if (!int.TryParse(Console.ReadLine(), out int days) || days < 0)
        {
            Console.WriteLine("Invalid minutes.");
            return;
        }

        Console.Write("Enter number of months to advance: ");
        if (!int.TryParse(Console.ReadLine(), out int months) || months < 0)
        {
            Console.WriteLine("Invalid minutes.");
            return;
        }

        Console.Write("Enter number of years to advance: ");
        if (!int.TryParse(Console.ReadLine(), out int years) || years < 0)
        {
            Console.WriteLine("Invalid minutes.");
            return;
        }

        try
        {
            for (int i = 0; i < minutes; i++)
                s_bl.Admin.ForwardClock(BO.TimeUnit.Minutes);

            for (int i = 0; i < hours; i++)
                s_bl.Admin.ForwardClock(BO.TimeUnit.Hours);

            for (int i = 0; i < days; i++)
                s_bl.Admin.ForwardClock(BO.TimeUnit.Days);

            for (int i = 0; i < months; i++)
                s_bl.Admin.ForwardClock(BO.TimeUnit.Months);

            for (int i = 0; i < years; i++)
                s_bl.Admin.ForwardClock(BO.TimeUnit.Years);

            Console.WriteLine($"\n Simulated clock successfully");
        }
        catch (BO.BlInvalidOperationException ex) { PrintException(ex); }
        catch (BO.BlException ex) { PrintException(ex); }
    }
    private static AdminMenuOptions ShowAdminMenu()
    {
        Console.WriteLine("\n--- ADMIN MENU ---");
        Console.WriteLine("0: Exit");
        Console.WriteLine("1: Reset Data");
        Console.WriteLine("2: Initialize Data");
        Console.WriteLine("3: Get current Clock Time");
        Console.WriteLine("4: Forward System Clock");
        Console.WriteLine("5: Get Configuration");
        Console.WriteLine("6: Set Configuration");

        int choice;
        while (!int.TryParse(Console.ReadLine(), out choice) || !Enum.IsDefined(typeof(AdminMenuOptions), choice))
        {
            Console.Write("Invalid input. Please enter a number between 0 and 6: ");
        }
        return (AdminMenuOptions)choice;
    }
    private static void printConfig(BO.Config config)
    {
        Console.WriteLine("\n--- Current Configuration ---");
        Console.WriteLine($"Current system clock: {config.Clock}");
        Console.WriteLine($"Maximum range for deliveries: {config.MaxDeliveryRange}");
        Console.WriteLine($"Risk range for deliveries: {config.RiskRange}");
        Console.WriteLine($"Inactivity time range: {config.InactivityTimeRange}");
        Console.WriteLine($"Company address: {config.CompenyAddress}");
        Console.WriteLine($"Company latitude: {config.CompenyLatitude}");
        Console.WriteLine($"Company longitude: {config.CompenyLongitude}");
        Console.WriteLine($"Admin ID: {config.AdminId}");
        Console.WriteLine($"Delivery max distance: {config.DeliveryMaxDistance}");
        Console.WriteLine($"Average vehicle speed (km/h): {config.AverageVehicleSpeedKmH}");
        Console.WriteLine($"Average motorcycle speed (km/h): {config.AverageMotorcycleSpeedKmH}");
        Console.WriteLine($"Average bicycle speed (km/h): {config.AverageBicycleSpeedKmH}");
        Console.WriteLine($"Average by foot speed (km/h): {config.AverageByFootSpeedKmH}");
    }
    private static void setConfigByAdmin()
    {
        Console.WriteLine("\n--- Update System Configuration ---");

        try
        {
            BO.Config oldConfig = s_bl.Admin.GetConfig();

            //Set Admin ID
            Console.Write("Enter new Admin ID: ");
            string? inputAdminId = Console.ReadLine();
            int newAdminId = oldConfig.AdminId;
            if (!string.IsNullOrEmpty(inputAdminId) && int.TryParse(inputAdminId, out int tempAdminId))
                newAdminId = tempAdminId;

            //Set Max Delivery Distance
            Console.Write("Enter new Max Distance for deliveries in KM: ");
            string? inputMaxDist = Console.ReadLine();
            double? newMaxDist = oldConfig.DeliveryMaxDistance;
            if (string.IsNullOrEmpty(inputMaxDist))
                newMaxDist = null;
            else if (double.TryParse(inputMaxDist, out double tempMaxDist))
                newMaxDist = tempMaxDist;
            else if (!string.IsNullOrEmpty(inputMaxDist))
                Console.WriteLine("Invalid number for Max Distance. Keeping old value.");

            //Set Average Vehicle Speed
            Console.Write("Enter new Car Speed (km/h): ");
            string? inputSpeedV = Console.ReadLine();
            double newSpeedV = oldConfig.AverageVehicleSpeedKmH;
            if (!string.IsNullOrEmpty(inputSpeedV) && double.TryParse(inputSpeedV, out double tempSpeedV))
                newSpeedV = tempSpeedV;
            else if (!string.IsNullOrEmpty(inputSpeedV))
                Console.WriteLine("Invalid number for Speed. Keeping old value.");

            //Set Average Motorcycle Speed
            Console.WriteLine("Enter new Motorcycle Speed (km/h): ");
            string? inputSpeedM = Console.ReadLine();
            double newSpeedM = oldConfig.AverageMotorcycleSpeedKmH;
            if (!string.IsNullOrEmpty(inputSpeedM) && double.TryParse(inputSpeedM, out double tempSpeedM))
                newSpeedM = tempSpeedM;
            else if (!string.IsNullOrEmpty(inputSpeedM))
                Console.WriteLine("Invalid number for Speed. Keeping old value.");

            //Set Average Bicycle Speed
            Console.WriteLine("Enter new Bicycle Speed (km/h): ");
            string? inputSpeedB = Console.ReadLine();
            double newSpeedB = oldConfig.AverageBicycleSpeedKmH;
            if (!string.IsNullOrEmpty(inputSpeedB) && double.TryParse(inputSpeedB, out double tempSpeedB))
                newSpeedB = tempSpeedB;
            else if (!string.IsNullOrEmpty(inputSpeedB))
                Console.WriteLine("Invalid number for Speed. Keeping old value.");

            //Set Average By Foot Speed
            Console.WriteLine("Enter new By Foot Speed (km/h): ");
            string? inputSpeedF = Console.ReadLine();
            double newSpeedF = oldConfig.AverageByFootSpeedKmH;
            if (!string.IsNullOrEmpty(inputSpeedF) && double.TryParse(inputSpeedF, out double tempSpeedF))
                newSpeedF = tempSpeedF;
            else if (!string.IsNullOrEmpty(inputSpeedF))
                Console.WriteLine("Invalid number for Speed. Keeping old value.");

            //Set Max Delivery Range
            Console.WriteLine("Enter new maximum range fo deliveries (in format 00:00:00): ");
            string? inputMaxDeliveryRange = Console.ReadLine();
            TimeSpan newMaxDeliveryRange = oldConfig.MaxDeliveryRange;
            if (!string.IsNullOrEmpty(inputMaxDeliveryRange) && TimeSpan.TryParse(inputMaxDeliveryRange, out TimeSpan tempMaxDeliveryRange))
                newMaxDeliveryRange = tempMaxDeliveryRange;
            else if (!string.IsNullOrEmpty(inputMaxDeliveryRange))
                Console.WriteLine("Invalid format for Max Delivery Range. Keeping old value.");

            //Set Risk Range
            Console.WriteLine("Enter new risk range (in format 00:00:00): ");
            string? inputRiskRange = Console.ReadLine();
            TimeSpan newRiskRange = oldConfig.RiskRange;
            if (!string.IsNullOrEmpty(inputRiskRange) && TimeSpan.TryParse(inputRiskRange, out TimeSpan tempRiskRange))
                newRiskRange = tempRiskRange;
            else if (!string.IsNullOrEmpty(inputRiskRange))
                Console.WriteLine("Invalid format for Risk Range. Keeping old value.");

            //Set Inactivity Time Range
            Console.WriteLine("Enter new inactivity time range (in format 00:00:00): ");
            string? inputInactivityTimeRange = Console.ReadLine();
            TimeSpan newInactivityTimeRange = oldConfig.InactivityTimeRange;
            if (!string.IsNullOrEmpty(inputInactivityTimeRange) && TimeSpan.TryParse(inputInactivityTimeRange, out TimeSpan tempInactivityTimeRange))
                newInactivityTimeRange = tempInactivityTimeRange;
            else if (!string.IsNullOrEmpty(inputInactivityTimeRange))
                Console.WriteLine("Invalid format for Inactivity Time Range. Keeping old value.");

            //create updated config object
            BO.Config updatedConfig = new BO.Config
            {
                AdminId = newAdminId,
                DeliveryMaxDistance = newMaxDist,
                AverageVehicleSpeedKmH = newSpeedV,
                CompenyAddress = oldConfig.CompenyAddress,
                CompenyLatitude = oldConfig.CompenyLatitude,
                CompenyLongitude = oldConfig.CompenyLongitude,
                AverageMotorcycleSpeedKmH = newSpeedM,
                AverageBicycleSpeedKmH = newSpeedB,
                AverageByFootSpeedKmH = newSpeedF,
                MaxDeliveryRange = newMaxDeliveryRange,
                RiskRange = newRiskRange,
                InactivityTimeRange = newInactivityTimeRange,
            };

            s_bl.Admin.SetConfig(updatedConfig);
            Console.WriteLine("\nConfiguration updated successfully.");
        }
        catch (BO.BlInvalidDataException ex) { PrintException(ex); }
        catch (BO.BlException ex) { PrintException(ex); }
    }
    private static void AdminMenu()
    {
        bool exit = false;
        while (!exit)
        {
            AdminMenuOptions choice = ShowAdminMenu();

            try
            {
                switch (choice)
                {
                    case AdminMenuOptions.Exit: exit = true; break;
                    case AdminMenuOptions.Reset: s_bl.Admin.ResetDB();
                        Console.WriteLine("Database reset successfully."); break;
                    case AdminMenuOptions.Initialize: s_bl.Admin.InitializeDB();
                        Console.WriteLine("Database initialized successfully."); break;
                    case AdminMenuOptions.GetClock: 
                        DateTime currentTime = s_bl.Admin.GetClock();
                        Console.WriteLine($"\nCurrent simulated clock time: {currentTime}"); break;
                    case AdminMenuOptions.ForwardClock: AdvanceClockOperation(); break;
                    case AdminMenuOptions.GetConfig:
                        var config = s_bl.Admin.GetConfig();
                        printConfig(config);
                        break;
                    case AdminMenuOptions.SetConfig: setConfigByAdmin(); break;
                }
            }
            catch (BO.BlException ex) { PrintException(ex); }
            catch (Exception ex) { Console.WriteLine($"An unexpected system error occurred: {ex.Message}"); }
        }
    }
    static void Main(string[] args)
    {
        try
        {
            Console.WriteLine("Initializing data...");
            s_bl.Admin.InitializeDB(); 
            Console.WriteLine("Data initialized successfully.");
        }
        catch (BO.BlException ex)
        {
            Console.WriteLine($"Critical error during BL initialization: {ex.Message}");
            Console.WriteLine("Press Enter to exit.");
            Console.ReadLine();
            return;
        }

        bool exit = false;
        while (!exit)
        {
            MainMenuOptions choice = ShowMainMenu();

            try
            {
                switch (choice)
                {
                    case MainMenuOptions.Exit: exit = true; break;
                    case MainMenuOptions.Admin: AdminMenu(); break;
                    case MainMenuOptions.Courier: CourierMenu(); break;
                    case MainMenuOptions.Order: OrderMenu(); break;
                }
            }
            catch (BO.BlException ex)
            {
                PrintException(ex);
                Console.WriteLine("Returning to main menu. Press Enter to continue...");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
                Console.WriteLine("Returning to main menu. Press Enter to continue...");
                Console.ReadLine();
            }
        }
    }
}