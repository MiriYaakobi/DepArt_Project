using BLApi;

namespace BlTest;

/// <summary>
/// the main program class for the console application
/// </summary>
/// <remarks>
/// a test console application for the business logic layer
/// </remarks>
internal class Program
{
    // create the business logic object
    static readonly IBl s_bl = Factory.Get();

    // the main entry point of the application
    private static int AdminID = s_bl.Admin.GetConfig().AdminId;

    //enumerations for menu options
    private enum MainMenuOptions
    {
        Exit,
        Admin,
        Courier,
        Order
    }

    //enumerations for courier options
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

    //enumerations for order options
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

    //enumerations for admin options
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

    /// <summary>
    /// shows the main menu and gets user choice
    /// </summary>
    /// <returns></returns>
    private static MainMenuOptions ShowMainMenu()
    {
        Console.WriteLine("\n--- MAIN MENU ---");
        Console.WriteLine("0: Exit");
        Console.WriteLine("1: Admin Management");
        Console.WriteLine("2: Courier Management");
        Console.WriteLine("3: Order Management");
        Console.Write("Enter your choice: ");

        int choice;

        // Validate input
        while (!int.TryParse(Console.ReadLine(), out choice) || !Enum.IsDefined(typeof(MainMenuOptions), choice))
        {
            Console.Write("Invalid input. Please enter a number between 0 and 3: ");
        }
        return (MainMenuOptions)choice;
    }

    /// <summary>
    /// shows the courier menu and gets user choice
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// shows the order menu and gets user choice
    /// </summary>
    /// <returns></returns>
    private static OrderMenuOptions ShowOrderMenu()
    {
        Console.WriteLine($"\n--- ORDER MENU ---");
        Console.WriteLine("0: Exit");
        Console.WriteLine("1: Create");
        Console.WriteLine("2: Read");
        Console.WriteLine("3: ReadAll");
        Console.WriteLine("4: Update");
        Console.WriteLine("5: Delete");
        Console.WriteLine("6: Cancel");
        Console.WriteLine("7: Orders Summary");
        Console.WriteLine("8: Report delivery completion");
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

    /// <summary>
    /// prints exception messages
    /// </summary>
    /// <param name="ex"></param>
    private static void PrintException(BO.BlException ex)
    {
        Console.WriteLine(ex.Message);

        // print inner exception message if exists
        if (ex.InnerException != null)
            Console.WriteLine(ex.InnerException.Message);
    }

    /// <summary>
    /// login courier operation method
    /// </summary>
    private static void LoginCourier()
    {
        Console.WriteLine("\n--- Courier Login ---");
        Console.Write("Enter Courier ID: ");

        //parse and validate ID input
        if (!int.TryParse(Console.ReadLine(), out int id))
        {
            Console.WriteLine("Invalid ID format.");
            return;
        }

        Console.Write("Enter Password: ");
        string? password = Console.ReadLine();

        // perform login
        try
        {
            BO.UserRole role = s_bl.Courier.Login(id, password!);

            // display login result
            if (role != BO.UserRole.None)
                Console.WriteLine($"\n Login successful. Welcome!");

            else
                Console.WriteLine("\n Login failed. Invalid ID or password.");
        }
        catch (BO.BlNotAuthorizedException ex) { PrintException(ex); }
    }

    /// <summary>
    /// Add courier operation method
    /// </summary>
    private static void AddCourier()
    {
        Console.WriteLine("\n--- Add New Courier ---");
        Console.Write("Enter Courier ID (9 digits): ");

        int inputId;
        bool isValid;

        //parse and validate ID input
        do
        {
            string? input = Console.ReadLine();
            isValid = int.TryParse(input, out inputId) && input?.Length == 9;

            if (!isValid)
                Console.WriteLine("Invalid ID. Please enter exactly 9 digits: ");

        } while (!isValid);

        Console.Write("Enter Full Name: ");
        string? name = Console.ReadLine();

        //validate name input
        while (string.IsNullOrWhiteSpace(name) ||
                               name.Length < 2 ||
                               !name.All(c => char.IsLetter(c) ||
                               char.IsWhiteSpace(c)))
        {
            Console.WriteLine("Invalid name. Please enter a full name (letters only, at least 2 characters).");
            name = Console.ReadLine();
        }

        Console.Write("Enter Phone Number: ");
        string? phone = Console.ReadLine();

        //validate phone input
        while (string.IsNullOrWhiteSpace(phone) || !phone.All(char.IsDigit) || phone.Length < 9 || phone.Length > 10)
        {
            Console.WriteLine("Invalid phone number. Please enter 9 or 10 digits only.");
            phone = Console.ReadLine();
        }

        Console.Write("Enter Email: ");
        string? email = Console.ReadLine();

        //validate email input
        while (string.IsNullOrWhiteSpace(email) || !email.Contains("@") || !email.Contains("."))
        {
            Console.WriteLine("Invalid email format. Please enter in example@example.example");
            email = Console.ReadLine();
        }

        Console.Write("Enter Password: ");
        string? password = Console.ReadLine();

        //validate password input
        while (string.IsNullOrWhiteSpace(password) || password.Length < 8)
        {
            Console.WriteLine("Password must be at least 8 characters.");
            password = Console.ReadLine();
        }

        Console.Write("Enter Delivery Type (Car, Motorcycle, Bicycle, ByFoot): ");
        BO.DeliveryType typeOfDelivery;

        //validate delivery type input
        while (!Enum.TryParse(Console.ReadLine(), true, out typeOfDelivery))
            Console.WriteLine("Invalid delivery type. Please choose: Car, Motorcycle, Bicycle, or ByFoot.");

        Console.Write("Enter Max Delivery Distance (or press Enter to skip): ");
        string? maxDistInput = Console.ReadLine();
        double? maxDist = null;

        //validate max distance input
        while (!string.IsNullOrEmpty(maxDistInput))
        {
            if (double.TryParse(maxDistInput, out double tempDistance) && tempDistance >= 0)
            {
                maxDist = tempDistance;
                break;
            }

            Console.WriteLine("Invalid distance. Please enter a positive number or press Enter to skip.");
            maxDistInput = Console.ReadLine();
        }

        try
        {
            BO.Courier newCourier = new BO.Courier
            {
                Id = inputId,
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

            Console.WriteLine($"\nSuccessfully added Courier {inputId} - {name}");
        }
        catch (BO.BlInvalidDataException ex) { PrintException(ex); } //validation failed
        catch (BO.BlAlreadyExistsException ex) { PrintException(ex); } //already exists
        catch (BO.BlNotAuthorizedException ex) { PrintException(ex); } //alout unauthorized
    }

    /// <summary>
    /// get courier operation method
    /// </summary>
    private static void GetCourier()
    {
        Console.WriteLine("\n--- Read Courier ---");
        Console.Write("Enter Courier ID to get: ");

        //parse and validate ID input
        if (!int.TryParse(Console.ReadLine(), out int id))
        {
            Console.WriteLine("Invalid ID format.");
            return;
        }

        try
        {
            BO.Courier? courier = s_bl.Courier.Read(AdminID, id);

            if (courier != null)
                Console.WriteLine($"\n Courier Details:\n{courier}");
        }
        catch (BO.BlDoesNotExistException ex) { PrintException(ex); }
        catch (BO.BlNotAuthorizedException ex) { PrintException(ex); }
    }

    /// <summary>
    /// get all couriers operation method
    /// </summary>
    private static void ListAllCouriers()
    {
        Console.WriteLine("\n--- List All Couriers ---");

        try
        {
            //get all couriers from business layer
            IEnumerable<BO.CourierInList> couriers = s_bl.Courier.ReadAll(AdminID);

            if (!couriers.Any())
            {
                Console.WriteLine("No couriers found.");
                return;
            }

            foreach (var courier in couriers)
                Console.WriteLine(courier);
        }
        catch (BO.BlNotAuthorizedException ex) { PrintException(ex); }
    }

    /// <summary>
    /// update courier operation method
    /// </summary>
    private static void UpdateCourier()
    {
        Console.WriteLine("\n--- Update Courier ---");
        Console.Write("Enter Courier ID to update: ");

        int inputId;
        bool isValid;

        //parse and validate ID input
        do
        {
            string? input = Console.ReadLine();
            isValid = int.TryParse(input, out inputId) && input?.Length == 9;

            if (!isValid)
                Console.Write("Invalid ID. Please enter exactly 9 digits: ");

        } while (!isValid);

        try
        {
            //get existing courier details
            BO.Courier oldCourier = s_bl.Courier.Read(AdminID, inputId)!;

            Console.WriteLine($"\nUpdating Courier {inputId}. (Press Enter to keep current value)");

            // --- Name ---
            Console.Write($"Enter new Name (Current: {oldCourier.Name}): ");
            string? newName = Console.ReadLine();

            //validate name input
            if (string.IsNullOrEmpty(newName))
                newName = oldCourier.Name;

            else
            {
                while (!newName.All(c => char.IsLetter(c) || char.IsWhiteSpace(c)) || newName.Length < 2)
                {
                    Console.Write("Invalid name. Letters only (min 2): ");
                    newName = Console.ReadLine();

                    if (string.IsNullOrEmpty(newName))
                    {
                        newName = oldCourier.Name;
                        break;
                    }
                }
            }

            // --- Phone ---
            Console.Write($"Enter new Phone (Current: {oldCourier.Phone}): ");
            string? newPhone = Console.ReadLine();

            //validate phone input
            if (string.IsNullOrEmpty(newPhone))
                newPhone = oldCourier.Phone;

            else
            {
                while (!newPhone.All(char.IsDigit) || newPhone.Length != 10)
                {
                    Console.Write("Invalid phone. Enter 10 digits: ");
                    newPhone = Console.ReadLine();

                    if (string.IsNullOrEmpty(newPhone))
                    {
                        newPhone = oldCourier.Phone;
                        break;
                    }
                }
            }

            // --- Email ---
            Console.Write($"Enter new Email (Current: {oldCourier.Email}): ");
            string? newEmail = Console.ReadLine();

            //validate email input
            if (string.IsNullOrEmpty(newEmail))
                newEmail = oldCourier.Email;

            else
            {
                while (!newEmail.Contains("@") || !newEmail.Contains("."))
                {
                    Console.Write("Invalid email format: ");
                    newEmail = Console.ReadLine();

                    if (string.IsNullOrEmpty(newEmail))
                    {
                        newEmail = oldCourier.Email;
                        break;
                    }
                }
            }

            // --- Password ---
            Console.Write("Enter new Password. must write a value: ");
            string? newPassword = Console.ReadLine();

            //validate password input
            while (string.IsNullOrEmpty(newPassword) || newPassword.Length < 8)
            {
                Console.Write("Password must be min 8 chars: ");
                newPassword = Console.ReadLine();
            }

            // --- IsActive ---
            Console.Write($"Active? YES/NO (Current: {(oldCourier.IsActive ? "YES" : "NO")}): ");
            string? activeInput = Console.ReadLine()?.ToUpper();
            bool newIsActive = oldCourier.IsActive;

            //validate active input
            if (!string.IsNullOrEmpty(activeInput))
            {
                while (activeInput != "YES" && activeInput != "NO")
                {
                    Console.Write("Please enter YES or NO: ");
                    activeInput = Console.ReadLine()?.ToUpper();

                    if (string.IsNullOrEmpty(activeInput))
                        break;
                }
                if (!string.IsNullOrEmpty(activeInput))
                    newIsActive = (activeInput == "YES");
            }

            // --- Delivery Type ---
            Console.Write($"Enter Shipping Type - Car/Motorcycle/Bicycle/ByFoot (Current: {oldCourier.TypeOfDelivery}): ");
            string? typeInput = Console.ReadLine();
            BO.DeliveryType newType = oldCourier.TypeOfDelivery;

            //validate delivery type input
            if (!string.IsNullOrEmpty(typeInput))
            {
                while (!Enum.TryParse(typeInput, true, out newType))
                {
                    Console.Write("Invalid type. Choose Car/Motorcycle/Bicycle/ByFoot: ");
                    typeInput = Console.ReadLine();

                    if (string.IsNullOrEmpty(typeInput))
                    {
                        newType = oldCourier.TypeOfDelivery;
                        break;
                    }
                }
            }

            // --- Max Distance ---
            Console.Write($"Enter new maximum distance (Current: {oldCourier.MaxDistance}): ");
            string? distInput = Console.ReadLine();
            double? newMaxDist = oldCourier.MaxDistance;

            //validate max distance input
            if (!string.IsNullOrEmpty(distInput))
            {
                double tempDist;
                while (!double.TryParse(distInput, out tempDist) || tempDist < 0)
                {
                    Console.Write("Invalid distance. Enter a positive number: ");
                    distInput = Console.ReadLine();
                    if (string.IsNullOrEmpty(distInput))
                    {
                        tempDist = oldCourier.MaxDistance ?? 0;
                        break;
                    }
                }
                if (!string.IsNullOrEmpty(distInput))
                    newMaxDist = tempDist;
            }

            BO.Courier updatedCourier = new BO.Courier
            {
                Id = oldCourier.Id,
                Name = newName,
                Phone = newPhone,
                Email = newEmail,
                Password = newPassword,
                IsActive = newIsActive,
                TypeOfDelivery = newType,
                MaxDistance = newMaxDist,
                TotalOnTimeDeliveries = oldCourier.TotalOnTimeDeliveries,
                TotalLateDeliveries = oldCourier.TotalLateDeliveries,
                CurrentOrder = oldCourier.CurrentOrder
            };

            s_bl.Courier.Update(AdminID, updatedCourier);

            Console.WriteLine($"\nSuccessfully updated Courier {inputId}");
        }
        catch (BO.BlDoesNotExistException ex) { PrintException(ex); }
        catch (BO.BlInvalidDataException ex) { PrintException(ex); }
        catch (BO.BlNotAuthorizedException ex) { PrintException(ex); }
    }

    /// <summary>
    /// delete courier operation method
    /// </summary>
    private static void DeleteCourier()
    {
        Console.WriteLine("\n--- Delete Courier ---");
        Console.Write("Enter Courier ID to delete: ");

        //parse and validate ID input
        if (!int.TryParse(Console.ReadLine(), out int id))
        {
            Console.WriteLine("Invalid ID format.");
            return;
        }

        try
        {
            s_bl.Courier.Delete(AdminID, id);
            Console.WriteLine($"\nSuccessfully deleted Courier {id}");
        }
        catch (BO.BlDoesNotExistException ex) { PrintException(ex); }
        catch (BO.BlCannotDeleteException ex) { PrintException(ex); }
        catch (BO.BlNotAuthorizedException ex) { PrintException(ex); }
    }

    /// <summary>
    /// the courier menu method
    /// </summary>
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
            catch (Exception ex) { Console.WriteLine($"ERROR: {ex.Message}"); }
        }
    }

    /// <summary>
    /// add order operation method
    /// </summary>
    private static void AddOrder()
    {
        Console.WriteLine("\n--- Add New Order ---");

        Console.Write("Enter Order Type (Regular, Express, SameDay): ");

        //validate order type input
        if (!Enum.TryParse(Console.ReadLine(), true, out BO.OrderType typeOfOrder))
        {
            Console.WriteLine("Invalid order type.");
            return;
        }

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

            Console.WriteLine($"\nOrder added Successfully");
        }
        catch (BO.BlInvalidDataException ex) { PrintException(ex); } //validation failed
        catch (BO.BlInvalidOperationException ex) { PrintException(ex); } //ID or Geocoding failed
        catch (BO.BlNotAuthorizedException ex) { PrintException(ex); } //alout unauthorized
    }

    /// <summary>
    /// get order operation method
    /// </summary>
    private static void GetOrder()
    {
        Console.WriteLine("\n--- Read Order ---");
        Console.Write("Enter Order ID to get: ");

        //parse and validate ID input
        if (!int.TryParse(Console.ReadLine(), out int id))
        {
            Console.WriteLine("Invalid ID format.");
            return;
        }

        try
        {
            BO.Order order = s_bl.Order.Read(AdminID, id);
            Console.WriteLine($"\n Order Details:\n{order}"); 
        }
        catch (BO.BlDoesNotExistException ex) { PrintException(ex); }
        catch (BO.BlNotAuthorizedException ex) { PrintException(ex); }
    }

    /// <summary>
    /// get all orders operation method
    /// </summary>
    private static void ListAllOrders()
    {
        Console.WriteLine("\n--- List All Orders ---");

        try
        {
            //get all orders from business layer
            IEnumerable<BO.OrderInList> orders = s_bl.Order.ReadAll(AdminID);

            //check if any orders exist
            if (!orders.Any())
            {
                Console.WriteLine("No orders found.");
                return;
            }

            //print each order
            foreach (var order in orders)
                Console.WriteLine(order);
        }
        catch (BO.BlNotAuthorizedException ex) { PrintException(ex); }
    }

    /// <summary>
    /// update order operation method
    /// </summary>
    private static void UpdateOrder()
    {
        Console.WriteLine("\n--- Update Order ---");
        Console.Write("Enter Order ID to update: ");

        //parse and validate ID input
        if (!int.TryParse(Console.ReadLine(), out int id))
        {
            Console.WriteLine("Invalid ID format.");
            return;
        }

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

    /// <summary>
    /// delete order operation method
    /// </summary>
    private static void DeleteOrder()
    {
        Console.WriteLine("\n--- Delete Order ---");
        Console.Write("Enter Order ID to delete: ");

        //parse and validate ID input
        if (!int.TryParse(Console.ReadLine(), out int id))
        {
            Console.WriteLine("Invalid ID format.");
            return;
        }

        try
        {
            s_bl.Order.Delete(AdminID, id);
            Console.WriteLine($"\nSuccessfully deleted Order {id}");
        }

        catch (BO.BlDoesNotExistException ex) { PrintException(ex); }
        catch (BO.BlInvalidOperationException ex) { PrintException(ex); }
        catch (BO.BlNotAuthorizedException ex) { PrintException(ex); }
    }

    /// <summary>
    /// cancel order operation method
    /// </summary>
    private static void CancelOrderOperation()
    {
        Console.WriteLine("\n--- Cancel Order ---");
        Console.Write("Enter Requesting User ID (Admin/Courier performing the action): ");

        //parse and validate ID input
        if (!int.TryParse(Console.ReadLine(), out int requestingUserId))
        {
            Console.WriteLine("Invalid User ID format.");
            return;
        }

        Console.Write("Enter Order ID to cancel: ");

        //parse and validate ID input
        if (!int.TryParse(Console.ReadLine(), out int orderId))
        {
            Console.WriteLine("Invalid Order ID format.");
            return;
        }

        try
        {
            // cancel the order
            s_bl.Order.Cancel(requestingUserId, orderId);
            Console.WriteLine($"\n Order {orderId} successfully canceled.");
        }

        catch (BO.BlNotAuthorizedException ex) { PrintException(ex); }
        catch (BO.BlDoesNotExistException ex) { PrintException(ex); }
        catch (BO.BlInvalidOperationException ex) { PrintException(ex); }
        catch (BO.BlException ex) { PrintException(ex); }
    }

    /// <summary>
    /// get order summary operation method
    /// </summary>
    private static void OrderSummaryOperation()
    {
        Console.WriteLine("\n--- Get Order Summary ---");

        Console.Write("Enter Requesting User ID: ");

        //parse and validate ID input
        if (!int.TryParse(Console.ReadLine(), out int requestingUserId))
        {
            Console.WriteLine("Invalid User ID format.");
            return;
        }

        try
        {
            // get order summary quantities
            int[] summary = s_bl.Order.GetOrderSummaryQuantities(requestingUserId);

            Console.WriteLine($"\nOrders Summary:");
            int statusCount = Enum.GetNames(typeof(BO.OrderStatus)).Length;

            // print summary
            for (int i = 0; i < summary.Length; i++)
            {
                BO.OrderStatus status = (BO.OrderStatus)(i % statusCount);
                BO.ScheduleStatus schedule = (BO.ScheduleStatus)(i / statusCount);

                // print only non-zero counts
                if (summary[i] > 0)
                    Console.WriteLine($"{status} - {schedule}: {summary[i]}");
            }

        }
        catch (BO.BlDoesNotExistException ex) { PrintException(ex); }
        catch (BO.BlNotAuthorizedException ex) { PrintException(ex); }
        catch (BO.BlException ex) { PrintException(ex); }
    }

    /// <summary>
    /// complete delivery operation method
    /// </summary>
    private static void CompleteDeliveryOperation()
    {
        Console.WriteLine("\n--- Report Delivery Completion ---");

        Console.Write("Enter Requesting User ID: ");

        //parse and validate ID input
        if (!int.TryParse(Console.ReadLine(), out int requestingUserId))
        {
            Console.WriteLine("Invalid User ID format.");
            return;
        }

        Console.Write("Enter Courier ID: ");

        //parse and validate ID input
        if (!int.TryParse(Console.ReadLine(), out int courierId))
        {
            Console.WriteLine("Invalid Courier ID format.");
            return;
        }

        Console.Write("Enter Delivery ID to complete: ");

        //parse and validate ID input
        if (!int.TryParse(Console.ReadLine(), out int deliveryId))
        {
            Console.WriteLine("Invalid Delivery ID format.");
            return;
        }

        try
        {
            s_bl.Order.CompleteDelivery(requestingUserId, courierId, deliveryId);
            Console.WriteLine($"\nDelivery {deliveryId} successfully marked as Delivered.");
        }
        catch (BO.BlNotAuthorizedException ex) { PrintException(ex); }
        catch (BO.BlDoesNotExistException ex) { PrintException(ex); }
        catch (BO.BlInvalidOperationException ex) { PrintException(ex); }
        catch (BO.BlException ex) { PrintException(ex); }
    }

    /// <summary>
    /// choose order operation method
    /// </summary>
    private static void ChooseOrderOperation()
    {
        Console.WriteLine("\n--- Choose Order for Delivery ---");

        Console.Write("Enter Requesting User ID (Admin/Courier performing the action): ");

        //parse and validate ID input
        if (!int.TryParse(Console.ReadLine(), out int requestingUserId))
        {
            Console.WriteLine("Invalid User ID format.");
            return;
        }

        Console.Write("Enter Courier ID who will perform the delivery: ");

        //parse and validate ID input
        if (!int.TryParse(Console.ReadLine(), out int courierId))
        {
            Console.WriteLine("Invalid Courier ID format.");
            return;
        }

        Console.Write("Enter Order ID to assign: ");

        //parse and validate ID input
        if (!int.TryParse(Console.ReadLine(), out int orderId))
        {
            Console.WriteLine("Invalid Order ID format.");
            return;
        }

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

    /// <summary>
    /// get closed deliveries for courier operation method
    /// </summary>
    private static void GetClosedDeliveriesForCourierOperation()
    {
        Console.WriteLine("\n--- View Closed Deliveries History ---");

        Console.Write("Enter Requesting User ID (Admin or Courier): ");

        //parse and validate ID input
        if (!int.TryParse(Console.ReadLine(), out int requestingUserId))
        {
            Console.WriteLine("Invalid User ID format.");
            return;
        }

        Console.Write("Enter Target Courier ID to view history for: ");

        //parse and validate ID input
        if (!int.TryParse(Console.ReadLine(), out int courierId))
        {
            Console.WriteLine("Invalid Courier ID format.");
            return;
        }

        try
        {
            // get closed deliveries from business layer
            IEnumerable<BO.ClosedDeliveryInList> closedDeliveries = s_bl.Order.GetClosedDeliveriesForCourier(requestingUserId, courierId);

            // check if any closed deliveries exist
            if (!closedDeliveries.Any())
            {
                Console.WriteLine($"\nCourier {courierId} has no completed delivery history.");
                return;
            }

            Console.WriteLine($"\n Closed Deliveries for Courier {courierId}:");

            // print each closed delivery
            foreach (var delivery in closedDeliveries)
            {
                Console.WriteLine(delivery);
            }
        }
        catch (BO.BlNotAuthorizedException ex) { PrintException(ex); }
        catch (BO.BlException ex) { PrintException(ex); }
    }

    /// <summary>
    /// get all open orders for courier operation method
    /// When writing this function, we used AI to ensure that the logic and sorting order were correct.
    /// </summary>
    private static void ListAllOpenOrders()
    {
        Console.WriteLine("\n--- List All Open Orders for Courier ---");

        Console.Write("Enter Requesting User ID (Admin or Courier): ");

        //parse and validate ID input
        if (!int.TryParse(Console.ReadLine(), out int requestingUserId))
        {
            Console.WriteLine("Invalid User ID format.");
            return;
        }

        Console.Write("Enter Courier ID to view open orders for: ");

        //parse and validate ID input
        if (!int.TryParse(Console.ReadLine(), out int courierId))
        { 
            Console.WriteLine("Invalid Courier ID format."); 
            return;
        }

        Console.WriteLine("\nFilter by Order Type? (Press 'Enter' for All)");
        Console.WriteLine("0: Regular, 1: Express, 2: SameDay");

        string? typeInput = Console.ReadLine();
        BO.OrderType? filterType = null;

        //validate order type input
        if (!string.IsNullOrWhiteSpace(typeInput) && int.TryParse(typeInput, out int typeVal))
        {
            if (Enum.IsDefined(typeof(BO.OrderType), typeVal))
                filterType = (BO.OrderType)typeVal;
        }

        Console.WriteLine("\nSort by? (Press 'Enter' for Default/Urgency)");
        Console.WriteLine("0: Id, 1: Timeliness, 4: AirDistance, 3: MaxDeliveryTime");

        string? sortInput = Console.ReadLine();
        BO.OpenOrderFieldSort? sortOption = null;

        //validate sort option input
        if (!string.IsNullOrWhiteSpace(sortInput) && int.TryParse(sortInput, out int sortVal))
        {
            if (Enum.IsDefined(typeof(BO.OpenOrderFieldSort), sortVal))
                sortOption = (BO.OpenOrderFieldSort)sortVal;
        }

        try
        {
            // get open orders from business layer
            IEnumerable<BO.OpenOrderInList> openOrders = s_bl.Order.ReadAllOpenOrders(requestingUserId, courierId, filterType, sortOption);

            if (!openOrders.Any())
            {
                Console.WriteLine($"\nCourier {courierId} has no matching open orders within range.");
                return;
            }

            Console.WriteLine($"\nFound {openOrders.Count()} open orders for Courier {courierId}:");

            // print each open order
            foreach (var order in openOrders)
                Console.WriteLine($"ID: {order.Id} \n Type: {order.TypeOfOrder} \n Dist: {order.AirDistance:F2}km \n Status: {order.TimeLinessStatus}");
        }
        catch (BO.BlNotAuthorizedException ex) { Console.WriteLine($"Error: {ex.Message}"); }
        catch (BO.BlDoesNotExistException ex) { Console.WriteLine($"Error: {ex.Message}"); }
        catch (BO.BlException ex) { Console.WriteLine($"Error: {ex.Message}"); }
    }

    /// <summary>
    /// order menu method
    /// </summary>
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

    /// <summary>
    /// advance clock operation method
    /// </summary>
    private static void AdvanceClockOperation()
    {
        Console.WriteLine("\n--- Advance Simulated Clock ---");

        Console.Write("Enter number of minutes to advance: ");

        //parse and validate minutes input
        if (!int.TryParse(Console.ReadLine(), out int minutes) || minutes < 0)
        {
            Console.WriteLine("Invalid minutes.");
            return;
        }

        Console.Write("Enter number of hours to advance: ");

        //parse and validate hours input
        if (!int.TryParse(Console.ReadLine(), out int hours) || hours < 0)
        {
            Console.WriteLine("Invalid hours.");
            return;
        }

        Console.Write("Enter number of days to advance: ");

        //parse and validate days input
        if (!int.TryParse(Console.ReadLine(), out int days) || days < 0)
        {
            Console.WriteLine("Invalid minutes.");
            return;
        }

        Console.Write("Enter number of months to advance: ");

        //parse and validate months input
        if (!int.TryParse(Console.ReadLine(), out int months) || months < 0)
        {
            Console.WriteLine("Invalid minutes.");
            return;
        }

        Console.Write("Enter number of years to advance: ");

        //parse and validate years input
        if (!int.TryParse(Console.ReadLine(), out int years) || years < 0)
        {
            Console.WriteLine("Invalid minutes.");
            return;
        }

        //advance the clock in the business layer
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

            Console.WriteLine($"\nSimulated clock successfully");
        }
        catch (BO.BlInvalidOperationException ex) { PrintException(ex); }
        catch (BO.BlException ex) { PrintException(ex); }
    }

    /// <summary>
    /// show admin menu method
    /// </summary>
    /// <returns></returns>
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
        Console.Write("Enter your choice: ");

        int choice;
        while (!int.TryParse(Console.ReadLine(), out choice) || !Enum.IsDefined(typeof(AdminMenuOptions), choice))
        {
            Console.Write("Invalid input. Please enter a number between 0 and 6: ");
        }
        return (AdminMenuOptions)choice;
    }

    /// <summary>
    /// print configuration method
    /// </summary>
    /// <param name="config"></param>
    private static void printConfig(BO.Config config)
    {
        Console.WriteLine("\n--- Current Configuration ---");
        Console.WriteLine($"Admin ID: {config.AdminId}");
        Console.WriteLine($"Current system clock: {config.Clock}");
        Console.WriteLine($"Maximum range for deliveries: {config.MaxDeliveryRange}");
        Console.WriteLine($"Risk range for deliveries: {config.RiskRange}");
        Console.WriteLine($"Inactivity time range: {config.InactivityTimeRange}");
        Console.WriteLine($"Company address: {config.CompenyAddress}");
        Console.WriteLine($"Company latitude: {config.CompenyLatitude}");
        Console.WriteLine($"Company longitude: {config.CompenyLongitude}");
        Console.WriteLine($"Delivery max distance: {config.DeliveryMaxDistance}");
        Console.WriteLine($"Average vehicle speed (km/h): {config.AverageVehicleSpeedKmH}");
        Console.WriteLine($"Average motorcycle speed (km/h): {config.AverageMotorcycleSpeedKmH}");
        Console.WriteLine($"Average bicycle speed (km/h): {config.AverageBicycleSpeedKmH}");
        Console.WriteLine($"Average by foot speed (km/h): {config.AverageByFootSpeedKmH}");
    }

    /// <summary>
    /// set configuration by admin operation method
    /// </summary>
    private static void setConfigByAdmin()
    {
        Console.WriteLine("\n--- Update System Configuration ---");

        try
        {
            //get old config from business layer to use as default values
            BO.Config oldConfig = s_bl.Admin.GetConfig();

            //Set Admin ID
            Console.Write("Enter new Admin ID (Enter to skip): ");
            string? inputAdminId = Console.ReadLine();
            int newAdminId = oldConfig.AdminId;

            //parse admin ID input
            if (!string.IsNullOrEmpty(inputAdminId) && int.TryParse(inputAdminId, out int tempAdminId))
                newAdminId = tempAdminId;

            //Set Max Delivery Distance
            Console.Write("Enter new Max Distance for deliveries in KM (Enter to skip): ");
            string? inputMaxDist = Console.ReadLine();
            double? newMaxDist = oldConfig.DeliveryMaxDistance;

            //parse max distance input
            if (double.TryParse(inputMaxDist, out double tempMaxDist))
                newMaxDist = tempMaxDist;

            //handle invalid input
            else if (!string.IsNullOrEmpty(inputMaxDist))
                Console.WriteLine("Invalid number for Max Distance. Keeping old value.");

            //Set Average Vehicle Speed
            Console.Write("Enter new Car Speed km/h (Enter to skip): ");
            string? inputSpeedV = Console.ReadLine();
            double newSpeedV = oldConfig.AverageVehicleSpeedKmH;

            //parse vehicle speed input
            if (!string.IsNullOrEmpty(inputSpeedV) && double.TryParse(inputSpeedV, out double tempSpeedV))
                newSpeedV = tempSpeedV;

            //handle invalid input
            else if (!string.IsNullOrEmpty(inputSpeedV))
                Console.WriteLine("Invalid number for Speed. Keeping old value.");

            //Set Average Motorcycle Speed
            Console.Write("Enter new Motorcycle Speed km/h (Enter to skip): ");
            string? inputSpeedM = Console.ReadLine();
            double newSpeedM = oldConfig.AverageMotorcycleSpeedKmH;

            //parse motorcycle speed input
            if (!string.IsNullOrEmpty(inputSpeedM) && double.TryParse(inputSpeedM, out double tempSpeedM))
                newSpeedM = tempSpeedM;

            //handle invalid input
            else if (!string.IsNullOrEmpty(inputSpeedM))
                Console.WriteLine("Invalid number for Speed. Keeping old value.");

            //Set Average Bicycle Speed
            Console.Write("Enter new Bicycle Speed km/h (Enter to skip): ");
            string? inputSpeedB = Console.ReadLine();
            double newSpeedB = oldConfig.AverageBicycleSpeedKmH;

            //parse bicycle speed input
            if (!string.IsNullOrEmpty(inputSpeedB) && double.TryParse(inputSpeedB, out double tempSpeedB))
                newSpeedB = tempSpeedB;

            //handle invalid input
            else if (!string.IsNullOrEmpty(inputSpeedB))
                Console.WriteLine("Invalid number for Speed. Keeping old value.");

            //Set Average By Foot Speed
            Console.Write("Enter new By Foot Speed km/h (Enter to skip): ");
            string? inputSpeedF = Console.ReadLine();
            double newSpeedF = oldConfig.AverageByFootSpeedKmH;

            //parse by foot speed input
            if (!string.IsNullOrEmpty(inputSpeedF) && double.TryParse(inputSpeedF, out double tempSpeedF))
                newSpeedF = tempSpeedF;

            //handle invalid input
            else if (!string.IsNullOrEmpty(inputSpeedF))
                Console.WriteLine("Invalid number for Speed. Keeping old value.");

            //Set Max Delivery Range
            Console.Write("Enter new maximum range fo deliveries in format 00:00:00 (Enter to skip): ");
            string? inputMaxDeliveryRange = Console.ReadLine();
            TimeSpan newMaxDeliveryRange = oldConfig.MaxDeliveryRange;

            //parse max delivery range input
            if (!string.IsNullOrEmpty(inputMaxDeliveryRange) && TimeSpan.TryParse(inputMaxDeliveryRange, out TimeSpan tempMaxDeliveryRange))
                newMaxDeliveryRange = tempMaxDeliveryRange;

            //handle invalid input
            else if (!string.IsNullOrEmpty(inputMaxDeliveryRange))
                Console.WriteLine("Invalid format for Max Delivery Range. Keeping old value.");

            //Set Risk Range
            Console.Write("Enter new risk range in format 00:00:00 (Enter to skip): ");
            string? inputRiskRange = Console.ReadLine();
            TimeSpan newRiskRange = oldConfig.RiskRange;

            //parse risk range input
            if (!string.IsNullOrEmpty(inputRiskRange) && TimeSpan.TryParse(inputRiskRange, out TimeSpan tempRiskRange))
                newRiskRange = tempRiskRange;

            //handle invalid input
            else if (!string.IsNullOrEmpty(inputRiskRange))
                Console.WriteLine("Invalid format for Risk Range. Keeping old value.");

            //Set Inactivity Time Range
            Console.Write("Enter new inactivity time range in format 00:00:00 (Enter to skip): ");
            string? inputInactivityTimeRange = Console.ReadLine();
            TimeSpan newInactivityTimeRange = oldConfig.InactivityTimeRange;

            //parse inactivity time range input
            if (!string.IsNullOrEmpty(inputInactivityTimeRange) && TimeSpan.TryParse(inputInactivityTimeRange, out TimeSpan tempInactivityTimeRange))
                newInactivityTimeRange = tempInactivityTimeRange;

            //handle invalid input
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

    /// <summary>
    /// admin menu method
    /// </summary>
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
            //catch (BO.BlException ex) { Console.WriteLine($"ERROR: {ex.Message}"); }
        }
    }

    /// <summary>
    /// the main entry point for the application.
    /// </summary>
    /// <param name="args"></param>
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
        }
    }
}