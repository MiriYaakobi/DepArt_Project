using BLApi;

namespace BlTest;

internal class Program
{
    static readonly IBl s_bl = BLApi.Factory.Get();
    private enum MainMenuOptions
    {
        Exit,
        Courier,
        Order,
        Delivery,
        Admin,
        InitializeData,
    }

    private enum CrudMenuOptions
    {
        Exit,
        Create,
        Read,
        ReadAll,
        Update,
        Delete
    }

    private enum AdminMenuOptions
    {
        Exit,
        AdvanceClock,
    }

    private static MainMenuOptions ShowMainMenu()
    {
        Console.WriteLine("\n--- MAIN MENU ---");
        Console.WriteLine("0: Exit");
        Console.WriteLine("1: Courier Management");
        Console.WriteLine("2: Order Management");
        Console.WriteLine("3: Delivery Operations");
        Console.WriteLine("4: Admin & Configuration");
        Console.WriteLine("5: Initialize Data");
        Console.Write("Enter your choice: ");

        int choice;
        while (!int.TryParse(Console.ReadLine(), out choice) || !Enum.IsDefined(typeof(MainMenuOptions), choice))
        {
            Console.Write("Invalid input. Please enter a number between 0 and 5: ");
        }
        return (MainMenuOptions)choice;
    }

    private static CrudMenuOptions ShowCrudMenu(string entityName)
    {
        Console.WriteLine($"\n--- {entityName.ToUpper()} MENU ---");
        Console.WriteLine("0: Back to Main Menu"); // Exit
        Console.WriteLine("1: Create (Add new)");
        Console.WriteLine("2: Read (Get by ID)");
        Console.WriteLine("3: ReadAll (List all)");
        Console.WriteLine("4: Update");
        Console.WriteLine("5: Delete (By ID)");
        Console.Write("Enter your choice: ");

        int choice;

        // Validate input
        while (!int.TryParse(Console.ReadLine(), out choice) || !Enum.IsDefined(typeof(CrudMenuOptions), choice))
        {
            Console.Write("Invalid input. Please enter a number between 0 and 6: ");
        }
        return (CrudMenuOptions)choice;
    }
    private static void PrintException(BO.BlException ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"\nERROR: {ex.GetType().Name}");
        Console.WriteLine($"Message: {ex.Message}");
        if (ex.InnerException != null)
        {
            Console.WriteLine($"Inner Exception: {ex.InnerException.GetType().Name} - {ex.InnerException.Message}");
        }
        Console.ResetColor();
    }

    private static void AddCourier()
    {
        Console.WriteLine("\n--- Add New Courier (BO) ---");

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

        Console.Write("Enter Max Delivery Distance (double): ");
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
            s_bl.Courier.Create(123, newCourier);

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
            BO.Courier? courier = s_bl.Courier.Read(123, id);

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
        Console.WriteLine("\n--- List All Couriers (BO.CourierInList) ---");

        try
        {
            IEnumerable<BO.CourierInList> couriers = s_bl.Courier.ReadAll(123);

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
        Console.WriteLine("\n--- Update Courier (BO) ---");
        Console.Write("Enter Courier ID to update: ");
        if (!int.TryParse(Console.ReadLine(), out int id)) { Console.WriteLine("Invalid ID format."); return; }

        try
        {
            BO.Courier oldCourier = s_bl.Courier.Read(123, id)!;

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

            s_bl.Courier.Update(123, updatedCourier);

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
            s_bl.Courier.Delete(123, id);

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
            CrudMenuOptions choice = ShowCrudMenu("Courier");

            try
            {
                switch (choice)
                {
                    case CrudMenuOptions.Exit: exit = true; break;
                    case CrudMenuOptions.Create: AddCourier(); break;
                    case CrudMenuOptions.Read: GetCourier(); break;
                    case CrudMenuOptions.ReadAll: ListAllCouriers(); break;
                    case CrudMenuOptions.Update: UpdateCourier(); break;
                    case CrudMenuOptions.Delete: DeleteCourier(); break;
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
            s_bl.Order.Create(123, newOrder);

            Console.WriteLine($"\n Successfully requested new order for {customerName}.");
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
            BO.Order order = s_bl.Order.Read(123, id);

            Console.WriteLine($"\n Order Details:\n{order}"); 
        }
        catch (BO.BlDoesNotExistException ex) { PrintException(ex); }
        catch (BO.BlNotAuthorizedException ex) { PrintException(ex); }
    }

    private static void ListAllOrders()
    {
        Console.WriteLine("\n--- List All Orders (BO.OrderInList) ---");

        try
        {
            IEnumerable<BO.OrderInList> orders = s_bl.Order.ReadAll(123);

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
            BO.Order oldOrder = s_bl.Order.Read(123, id)!;

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

            s_bl.Order.Update(123, updatedOrder);

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
        Console.Write("Enter Order ID to delete (Note: Deletion is often disallowed by BL): ");
        if (!int.TryParse(Console.ReadLine(), out int id)) { Console.WriteLine("Invalid ID format."); return; }

        try
        {
            s_bl.Order.Delete(123, id);

            Console.WriteLine($"\n✅ Successfully deleted Order {id}");
        }
        catch (BO.BlDoesNotExistException ex) { PrintException(ex); }
        catch (BO.BlInvalidOperationException ex) { PrintException(ex); }
        catch (BO.BlNotAuthorizedException ex) { PrintException(ex); }
    }

    private static void OrderMenu()
    {
        bool exit = false;
        while (!exit)
        {
            CrudMenuOptions choice = ShowCrudMenu("Order (BL)");

            try
            {
                switch (choice)
                {
                    case CrudMenuOptions.Exit: exit = true; break;
                    case CrudMenuOptions.Create: AddOrder(); break;
                    case CrudMenuOptions.Read: GetOrder(); break;
                    case CrudMenuOptions.ReadAll: ListAllOrders(); break;
                    case CrudMenuOptions.Update: UpdateOrder(); break;
                    case CrudMenuOptions.Delete: DeleteOrder(); break;
                }
            }
            catch (BO.BlException ex) { PrintException(ex); }
            catch (Exception ex) { Console.WriteLine($"An unexpected system error occurred: {ex.Message}"); }
        }
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

            Console.WriteLine($"\n✅ Order {orderId} successfully assigned to Courier {courierId}. Delivery process initiated.");
        }
        catch (BO.BlNotAuthorizedException ex) { PrintException(ex); }
        catch (BO.BlDoesNotExistException ex) { PrintException(ex); }
        catch (BO.BlInvalidOperationException ex) { PrintException(ex); }
        catch (BO.BlException ex) { PrintException(ex); }
    }
    private static void CompleteDeliveryOperation()
    {
        Console.WriteLine("\n--- Report Delivery Completion ---");

        Console.Write("Enter Courier ID reporting completion: ");
        if (!int.TryParse(Console.ReadLine(), out int courierId)) { Console.WriteLine("Invalid Courier ID format."); return; }

        Console.Write("Enter Delivery ID to close: ");
        if (!int.TryParse(Console.ReadLine(), out int deliveryId)) { Console.WriteLine("Invalid Delivery ID format."); return; }

        Console.Write("Enter Delivery End Latitude: ");
        if (!double.TryParse(Console.ReadLine(), out double endLat)) { Console.WriteLine("Invalid Latitude format."); return; }

        Console.Write("Enter Delivery End Longitude: ");
        if (!double.TryParse(Console.ReadLine(), out double endLon)) { Console.WriteLine("Invalid Longitude format."); return; }

        try
        {

            s_bl.Order.CompleteDelivery(courierId, courierId, deliveryId, endLat, endLon);

            Console.WriteLine($"\n✅ Delivery {deliveryId} successfully marked as Delivered by Courier {courierId}.");
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

            Console.WriteLine($"\n✅ Closed Deliveries for Courier {courierId}:");
            foreach (var delivery in closedDeliveries)
            {
                Console.WriteLine(delivery);
            }
        }
        catch (BO.BlNotAuthorizedException ex) { PrintException(ex); }
        catch (BO.BlException ex) { PrintException(ex); }
    }


    private enum DeliveryOperationsMenuOptions
    {
        Exit,
        ChooseOrder,
        CompleteDelivery,
        ViewClosedHistory
    }

    private static DeliveryOperationsMenuOptions ShowDeliveryMenu()
    {
        Console.WriteLine("\n--- DELIVERY OPERATIONS MENU ---");
        Console.WriteLine("0: Back to Main Menu");
        Console.WriteLine("1: Choose Order (Assign Delivery)");
        Console.WriteLine("2: Complete Delivery (Report Delivery)");
        Console.WriteLine("3: View Courier's Closed Deliveries History");
        Console.Write("Enter your choice: ");

        int choice;
        while (!int.TryParse(Console.ReadLine(), out choice) || !Enum.IsDefined(typeof(DeliveryOperationsMenuOptions), choice))
        {
            Console.Write("Invalid input. Please enter a number between 0 and 3: ");
        }
        return (DeliveryOperationsMenuOptions)choice;
    }

    private static void DeliveryMenu()
    {
        bool exit = false;
        while (!exit)
        {
            DeliveryOperationsMenuOptions choice = ShowDeliveryMenu();

            try
            {
                switch (choice)
                {
                    case DeliveryOperationsMenuOptions.Exit: exit = true; break;
                    case DeliveryOperationsMenuOptions.ChooseOrder: ChooseOrderOperation(); break;
                    case DeliveryOperationsMenuOptions.CompleteDelivery: CompleteDeliveryOperation(); break;
                    case DeliveryOperationsMenuOptions.ViewClosedHistory: GetClosedDeliveriesForCourierOperation(); break;
                }
            }
            catch (BO.BlException ex) { PrintException(ex); }
            catch (Exception ex) { Console.WriteLine($"An unexpected system error occurred: {ex.Message}"); }
        }
    }

    private static void AdvanceClockOperation()
    {
        Console.WriteLine("\n--- Advance Simulated Clock ---");

        Console.Write("Enter number of hours to advance: ");
        if (!int.TryParse(Console.ReadLine(), out int hours) || hours < 0)
        {
            Console.WriteLine("Invalid hours.");
            return;
        }

        Console.Write("Enter number of minutes to advance: ");
        if (!int.TryParse(Console.ReadLine(), out int minutes) || minutes < 0)
        {
            Console.WriteLine("Invalid minutes.");
            return;
        }

        try
        {
            s_bl.Admin.ForwardClock(BO.TimeUnit.Hours);

            Console.WriteLine($"\n Simulated clock successfully advanced by {hours} hours and {minutes} minutes.");
        }
        catch (BO.BlInvalidOperationException ex) { PrintException(ex); }
        catch (BO.BlException ex) { PrintException(ex); }
    }

    private static AdminMenuOptions ShowAdminMenu()
    {
        Console.WriteLine("\n--- ADMIN / CLOCK MENU ---");
        Console.WriteLine("0: Back to Main Menu");
        Console.WriteLine("1: Advance Simulated Clock");
        Console.Write("Enter your choice: ");

        int choice;
        while (!int.TryParse(Console.ReadLine(), out choice) || !Enum.IsDefined(typeof(AdminMenuOptions), choice))
        {
            Console.Write("Invalid input. Please enter a number between 0 and 1: ");
        }
        return (AdminMenuOptions)choice;
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
                    case AdminMenuOptions.AdvanceClock: AdvanceClockOperation(); break;
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
            Console.WriteLine("Initializing BL data...");
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
                    case MainMenuOptions.Courier: CourierMenu(); break;
                    case MainMenuOptions.Order: OrderMenu(); break;
                    case MainMenuOptions.Delivery: DeliveryMenu(); break;
                    case MainMenuOptions.Admin: AdminMenu(); break;
                    case MainMenuOptions.InitializeData:
                        Console.WriteLine("Re-initializing data...");
                        s_bl.Admin.InitializeDB();
                        Console.WriteLine("Data re-initialized successfully.");
                        break;
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