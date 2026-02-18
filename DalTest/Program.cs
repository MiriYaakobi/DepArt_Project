using DalApi;
using DO;
namespace DalTest;

/// <summary>
/// Main entry point for the application
/// We used AI for the blockchain, writing and editing them ourselves.
/// </summary>
internal class Program
{
    //create DAL instances for each entity
    //static readonly IDal s_dal = new Dal.DalList(); // from stage 2
    //static readonly IDal s_dal = new Dal.DalXml(); // from stage 3
    static readonly IDal s_dal = Factory.Get;

    /// <summary>
    /// Enum for main menu options.
    /// </summary>
    private enum MainMenuOptions
    {
        Exit,
        Courier,
        Order,
        Delivery,
        Config,
        InitializeData,
        ResetData,
        ListAllData
    }

    /// <summary>
    /// Enum for CRUD menu options.
    /// </summary>
    private enum CrudMenuOptions
    {
        Exit,
        Create,
        Read,
        ReadAll,
        Update,
        Delete,
        DeleteAll
    }

    /// <summary>
    /// Enum for configuration menu options.
    /// </summary>
    private enum ConfigMenuOptions
    {
        Exit,
        AddMinute,
        AddHour,
        AddDay,
        AddWeek,
        AddMonth,
        AddYear,
        ShowCurrentClock,
        UpdateVariable,
        ShowVariable,
        ResetAllConfig
    }

    /// <summary>
    /// Displays the main menu and returns the user's selection.
    /// </summary>
    private static MainMenuOptions ShowMainMenu()
    {
        Console.WriteLine("\n--- MAIN MENU ---");
        Console.WriteLine("0: Exit");
        Console.WriteLine("1: Courier Menu");
        Console.WriteLine("2: Order Menu");
        Console.WriteLine("3: Delivery Menu");
        Console.WriteLine("4: Config Menu");
        Console.WriteLine("5: Initialize Data (Reset and Create)");
        Console.WriteLine("6: Reset All Data (Clear Lists & Config)");
        Console.WriteLine("7: List All Data");
        Console.Write("Enter your choice: ");

        int choice;

        // Validate input
        while (!int.TryParse(Console.ReadLine(), out choice) || !Enum.IsDefined(typeof(MainMenuOptions), choice))
        {
            Console.Write("Invalid input. Please enter a number between 0 and 7: ");
        }
        return (MainMenuOptions)choice;
    }

    /// <summary>
    /// Displays the CRUD menu for a specific entity.
    /// </summary>
    /// <param name="entityName">The name of the entity to display in the title (e.g., "Courier").</param>
    private static CrudMenuOptions ShowCrudMenu(string entityName)
    {
        Console.WriteLine($"\n--- {entityName.ToUpper()} MENU ---");
        Console.WriteLine("0: Back to Main Menu"); // Exit
        Console.WriteLine("1: Create (Add new)");
        Console.WriteLine("2: Read (Get by ID)");
        Console.WriteLine("3: ReadAll (List all)");
        Console.WriteLine("4: Update");
        Console.WriteLine("5: Delete (By ID)");
        Console.WriteLine("6: DeleteAll (Clear list)");
        Console.Write("Enter your choice: ");

        int choice;

        // Validate input
        while (!int.TryParse(Console.ReadLine(), out choice) || !Enum.IsDefined(typeof(CrudMenuOptions), choice))
        {
            Console.Write("Invalid input. Please enter a number between 0 and 6: ");
        }
        return (CrudMenuOptions)choice;
    }

    /// <summary>
    /// Displays the configuration menu and returns the user's selection.
    /// </summary>
    private static ConfigMenuOptions ShowConfigMenu()
    {
        Console.WriteLine("\n--- CONFIGURATION MENU ---");
        Console.WriteLine("0: Back to Main Menu"); // Exit
        Console.WriteLine("1: Add Minute");
        Console.WriteLine("2: Add Hour");
        Console.WriteLine("3: Add Day");
        Console.WriteLine("4: Add Week");
        Console.WriteLine("5: Add Month");
        Console.WriteLine("6: Add Year");
        Console.WriteLine("7: Show Current Clock");
        Console.WriteLine("8: Update Config Variable");
        Console.WriteLine("9: Show Config Variable");
        Console.WriteLine("10: Reset All Config");
        Console.Write("Enter your choice: ");

        int choice;

        // Validate input
        while (!int.TryParse(Console.ReadLine(), out choice) || !Enum.IsDefined(typeof(ConfigMenuOptions), choice))
        {
            Console.Write("Invalid input. Please enter a number between 0 and 10: ");
        }
        return (ConfigMenuOptions)choice;
    }


    /// <summary>
    /// Helper function to add a new courier
    /// </summary>
    private static void AddCourier()
    {
        try
        {
            Console.Write("Enter Courier ID (ID Card): ");
            int id;
            while (!int.TryParse(Console.ReadLine(), out id))
                Console.Write("Invalid input. Please enter a valid number for ID: ");

            Console.Write("Enter Full Name: ");
            string? name = Console.ReadLine();

            Console.Write("Enter Phone Number: ");
            string? phone = Console.ReadLine();

            Console.Write("Enter Email: ");
            string? email = Console.ReadLine();

            Console.Write("Enter Password: ");
            string? password = Console.ReadLine();

            Console.Write("Is Active (true/false): ");
            bool isActive;
            while (!bool.TryParse(Console.ReadLine(), out isActive))
                Console.Write("Invalid input. Please enter 'true' or 'false': ");

            Console.Write("Enter Delivery Type (Car, Motorcycle, Bicycle, ByFoot): ");
            DO.DeliveryType typeOfDelivery;
            while (!Enum.TryParse(Console.ReadLine(), true, out typeOfDelivery))
                Console.Write("Invalid type. Please enter (Car, Motorcycle, Bicycle, ByFoot): ");

            // Get the start work time from the configuration
            DateTime startWorkTime = s_dal!.Config.Clock;

            Console.Write("Enter Max Delivery Distance (leave empty for no limit): ");
            string? maxDistInput = Console.ReadLine();
            double? maxDist = null;

            // Parse max distance if provided
            if (!string.IsNullOrEmpty(maxDistInput))
            {
                double tempDistance;

                // Validate input
                while (!double.TryParse(maxDistInput, out tempDistance))
                {
                    Console.Write("Invalid number. Enter Max Distance (or leave empty): ");
                    maxDistInput = Console.ReadLine();
                    if (string.IsNullOrEmpty(maxDistInput)) break;
                }
                if (!string.IsNullOrEmpty(maxDistInput))
                    maxDist = tempDistance;
            }

            // Create the new courier object
            s_dal!.Courier.Create(new(id, name!, phone!, email!, password!, isActive, typeOfDelivery, startWorkTime, maxDist));

            Console.WriteLine($"Successfully added Courier {id} - {name}");
        }

        catch (DalAlreadyExistsException ex)
        {
            Console.WriteLine($"Error adding courier: {ex.Message}");
        }
    }

    /// <summary>
    /// Retrieves and displays a courier by ID.
    /// </summary>
    private static void GetCourier()
    {
        try
        {
            Console.Write("Enter Courier ID to get: ");
            int id;

            // Validate input
            while (!int.TryParse(Console.ReadLine(), out id))
                Console.Write("Invalid input. Please enter a valid number for ID: ");

            //call to DAL
            DO.Courier? courier = s_dal!.Courier.Read(id);

            // Check if courier was found
            if (courier == null)
            {
                Console.WriteLine($"Courier with ID={id} not found.");
                return;
            }

            Console.WriteLine(courier); 
        }
        catch (DalNullValueException ex)
        {
            Console.WriteLine($"Error getting courier: {ex.Message}");
        }
        catch (DalDoesNotExistException ex)
        {
            Console.WriteLine($"Error getting courier: {ex.Message}");
        }
    }

    /// <summary>
    /// Retrieves and displays all couriers.
    /// </summary>
    private static void ListAllCouriers()
    {
        try
        {
            //IEnumerable<DO.Courier> couriers = s_dal.Courier.ReadAll();

            List<DO.Courier> couriers = s_dal!.Courier.ReadAll().ToList();

            // Check if any couriers were found
            if (couriers.Count == 0)
            {
                Console.WriteLine("No couriers found in the database.");
                return;
            }

            foreach (var courier in couriers)
            {
                Console.WriteLine(courier);
            }
        }
       
        catch (DalNullValueException ex)
        {
            Console.WriteLine($"Error listing couriers: {ex.Message}");
        }
        catch (DalDoesNotExistException ex)
        {
            Console.WriteLine($"Error listing couriers: {ex.Message}");
        }
    }

    /// <summary>
    /// Retrieves and updates an existing courier.
    /// </summary>
    private static void UpdateCourier()
    {
        try
        {
            Console.Write("Enter Courier ID to update: ");
            int id;

            // Validate input
            while (!int.TryParse(Console.ReadLine(), out id))
                Console.Write("Invalid input. Please enter a valid number for ID: ");

            // Read the existing courier
            DO.Courier? oldCourier = s_dal!.Courier.Read(id);
            if (oldCourier == null)
            {
                Console.WriteLine($"Courier with ID={id} not found.");
                return;
            }

            Console.WriteLine("Current values:");
            Console.WriteLine(oldCourier);

            // Read new values
            DO.Courier updatedCourier = oldCourier with { };

            Console.Write($"Enter new Name (current: {oldCourier.Name}): ");
            string? newName = Console.ReadLine();
            if (!string.IsNullOrEmpty(newName))
                updatedCourier = updatedCourier with { Name = newName! };

            Console.Write($"Enter new Phone (current: {oldCourier.Phone}): ");
            string? newPhone = Console.ReadLine();
            if (!string.IsNullOrEmpty(newPhone))
                updatedCourier = updatedCourier with { Phone = newPhone! };

            Console.Write($"Enter new Email (current: {oldCourier.Email}): ");
            string? newEmail = Console.ReadLine();
            if (!string.IsNullOrEmpty(newEmail))
                updatedCourier = updatedCourier with { Email = newEmail! };

            Console.Write($"Enter new Password (current: *****): ");
            string? newPassword = Console.ReadLine();
            if (!string.IsNullOrEmpty(newPassword))
                updatedCourier = updatedCourier with { Password = newPassword! };

            Console.Write($"Enter new IsActive (current: {oldCourier.IsActive}) (true/false): ");
            string? newIsActiveInput = Console.ReadLine();

            // Validate and update IsActive
            if (!string.IsNullOrEmpty(newIsActiveInput))
            {
                bool newIsActive;

                // Validate input
                while (!bool.TryParse(newIsActiveInput, out newIsActive))
                {
                    Console.Write("Invalid. Enter 'true' or 'false' (or leave empty): ");
                    newIsActiveInput = Console.ReadLine();
                    if (string.IsNullOrEmpty(newIsActiveInput)) break;
                }
                // Update if valid
                if (!string.IsNullOrEmpty(newIsActiveInput))
                    updatedCourier = updatedCourier with { IsActive = newIsActive };
            }

            Console.Write($"Enter new Delivery Type (current: {oldCourier.TypeOfDelivery}): ");
            string? newTypeInput = Console.ReadLine();
            // Validate and update Delivery Type
            if (!string.IsNullOrEmpty(newTypeInput))
            {
                DO.DeliveryType newType;
                // Validate input
                while (!Enum.TryParse(newTypeInput, true, out newType))
                {
                    Console.Write("Invalid. Enter Type (Vehicle, Motorcycle, Bicycle, ByFoot) (or leave empty): ");
                    newTypeInput = Console.ReadLine();
                    if (string.IsNullOrEmpty(newTypeInput)) break;
                }
                // Update if valid
                if (!string.IsNullOrEmpty(newTypeInput))
                    updatedCourier = updatedCourier with { TypeOfDelivery = newType };
            }

            Console.Write($"Enter new Max Delivery Distance (current: {oldCourier.MaxDistance}): ");
            string? newMaxDistInput = Console.ReadLine();

            // Validate and update Max Distance
            if (!string.IsNullOrEmpty(newMaxDistInput))
            {
                double newMaxDist;

                // Validate input
                while (!double.TryParse(newMaxDistInput, out newMaxDist))
                {
                    Console.Write("Invalid number. Enter Max Distance (or leave empty): ");
                    newMaxDistInput = Console.ReadLine();
                    if (string.IsNullOrEmpty(newMaxDistInput)) break;
                }
                // Update if valid
                if (!string.IsNullOrEmpty(newMaxDistInput))
                    updatedCourier = updatedCourier with { MaxDistance = newMaxDist };
            }

            s_dal!.Courier.Update(updatedCourier);

            Console.WriteLine($"Successfully updated Courier {id}");
        }
        catch (DalNullValueException ex)
        {
            Console.WriteLine($"Error updating courier: {ex.Message}");
        }
        catch (DalAlreadyExistsException ex)
        {
            Console.WriteLine($"Error updating courier: {ex.Message}");
        }
        catch (DalDoesNotExistException ex)
        {
            Console.WriteLine($"Error updating courier: {ex.Message}");
        }
    }

    /// <summary>
    /// Helper function to delete a courier by ID
    /// </summary>
    private static void DeleteCourier()
    {
        try
        {
            Console.Write("Enter Courier ID to delete: ");
            int id;

            // Validate input
            while (!int.TryParse(Console.ReadLine(), out id))
                Console.Write("Invalid input. Please enter a valid number for ID: ");

            // Call to DAL
            s_dal!.Courier.Delete(id);

            Console.WriteLine($"Successfully deleted Courier {id}");
        }
        catch (DalNullValueException ex)
        {
            Console.WriteLine($"Error deleting courier: {ex.Message}");
        }
        catch (DalDoesNotExistException ex)
        {
            Console.WriteLine($"Error deleting courier: {ex.Message}");
        }
    }

    /// <summary>
    /// Helper function to delete all couriers
    /// </summary>
    private static void DeleteAllCouriers()
    {
        try
        {
            // Call to DAL
            s_dal!.Courier.DeleteAll();

            Console.WriteLine("Successfully deleted all couriers.");
        }
        catch (DalNullValueException ex)
        {
            Console.WriteLine($"Error deleting all couriers: {ex.Message}");
        }
        catch (DalDoesNotExistException ex)
        {
            Console.WriteLine($"Error deleting all couriers: {ex.Message}");
        }
    }

    /// <summary>
    /// Helper function to display the courier management menu
    /// </summary>
    private static void CourierMenu()
    {
        bool exit = false;

        // Loop until the user chooses to exit
        while (!exit)
        {
            CrudMenuOptions choice = ShowCrudMenu("Courier");

            // Handle the user's choice
            switch (choice)
            {
                case CrudMenuOptions.Exit:
                    exit = true;
                    break;
                case CrudMenuOptions.Create:
                    AddCourier(); 
                    break;
                case CrudMenuOptions.Read:
                    GetCourier();
                    break;
                case CrudMenuOptions.ReadAll:
                    ListAllCouriers();
                    break;
                case CrudMenuOptions.Update:
                    UpdateCourier();
                    break;
                case CrudMenuOptions.Delete:
                    DeleteCourier(); 
                    break;
                case CrudMenuOptions.DeleteAll:
                    DeleteAllCouriers(); 
                    break;
            }
        }
    }


    /// <summary>
    /// Helper function to add a new order
    /// </summary>
    private static void AddOrder()
    {
        try
        {
            Console.Write("Enter Order Type (Regular, Express, SameDay): ");
            DO.OrderType typeOfOrder;

            // Validate input
            while (!Enum.TryParse(Console.ReadLine(), true, out typeOfOrder))
                Console.Write("Invalid type. Please enter (Regular, Express, SameDay): ");

            Console.Write("Enter Address: ");
            string? address = Console.ReadLine();

            Console.Write("Enter Latitude (Geographical coordinate): ");
            double latitude;

            // Validate input
            while (!double.TryParse(Console.ReadLine(), out latitude))
                Console.Write("Invalid input. Please enter a valid number for latitude: ");

            Console.Write("Enter Longitude (Geographical coordinate): ");
            double longitude;

            // Validate input
            while (!double.TryParse(Console.ReadLine(), out longitude))
                Console.Write("Invalid input. Please enter a valid number for longitude: ");

            Console.Write("Enter Customer Name: ");
            string? customerName = Console.ReadLine();

            Console.Write("Enter Customer Phone: ");
            string? customerPhone = Console.ReadLine();

            int id = 0;

            // Get the order opening time from the configuration
            DateTime orderOpeningTime = s_dal!.Config.Clock;

            Console.Write("Enter Package Details (optional, press Enter to skip): ");
            string? packageDetails = Console.ReadLine();

            Console.Write("Enter Description (optional, press Enter to skip): ");
            string? description = Console.ReadLine();

            // Create the new order object
            s_dal!.Order!.Create(new(id, typeOfOrder, address!, latitude, longitude, customerName!, customerPhone!, orderOpeningTime, packageDetails, description));

            Console.WriteLine($"Successfully added new order for {customerName}");
        }
        catch (DalAlreadyExistsException ex)
        {
            Console.WriteLine($"Error adding order: {ex.Message}");
        }
    }

    /// <summary>
    /// Helper function to get and display an order by ID
    /// </summary>
    private static void GetOrder()
    {
        try
        {
            Console.Write("Enter Order ID to get: ");
            int id;
            while (!int.TryParse(Console.ReadLine(), out id)) 
                Console.Write("Invalid input. Please enter a valid number for ID: ");

            // Call to DAL
            DO.Order? order = s_dal!.Order.Read(id);

            // Check if order was found
            if (order == null)
            {
                Console.WriteLine($"Order with ID={id} not found.");
                return;
            }

            Console.WriteLine(order); 
        }
        catch (DalNullValueException ex)
        {
            Console.WriteLine($"Error getting order: {ex.Message}");
        }
        catch (DalDoesNotExistException ex)
        {
            Console.WriteLine($"Error getting order: {ex.Message}");
        }
    }

    /// <summary>
    /// Helper function to list all orders
    /// </summary>
    private static void ListAllOrders()
    {
        try
        {
            // Call to DAL
            List<DO.Order> orders = s_dal!.Order.ReadAll().ToList();

            // Check if any orders were found
            if (orders.Count == 0)
            {
                Console.WriteLine("No orders found in the database.");
                return;
            }

            // Display each order
            foreach (var order in orders)
            {
                Console.WriteLine(order);
            }
        }
        catch (DalNullValueException ex)
        {
            Console.WriteLine($"Error listing orders: {ex.Message}");
        }
        catch (DalDoesNotExistException ex)
        {
            Console.WriteLine($"Error listing orders: {ex.Message}");
        }
    }

    /// <summary>
    /// Helper function to update an existing order
    /// </summary>
    private static void UpdateOrder()
    {
        try
        {
            // Get the order ID and read the object
            Console.Write("Enter Order ID to update: ");
            int id;

            // Validate input
            while (!int.TryParse(Console.ReadLine(), out id))
                Console.Write("Invalid input. Please enter a valid number for ID: ");

            // Read the existing order
            DO.Order? oldOrder = s_dal!.Order.Read(id);

            // Check if order was found
            if (oldOrder == null)
            {
                Console.WriteLine($"Order with ID={id} not found.");
                return;
            }

            Console.WriteLine("Current values:");
            Console.WriteLine(oldOrder);

            // Read new values
            DO.Order updatedOrder = oldOrder with { };

            Console.Write($"Enter new Order Type (current: {oldOrder.TypeOfOrder}): ");
            string? newTypeInput = Console.ReadLine();

            // Validate and update Order Type
            if (!string.IsNullOrEmpty(newTypeInput))
            {
                DO.OrderType newType;

                // Validate input
                while (!Enum.TryParse(newTypeInput, true, out newType))
                {
                    Console.Write("Invalid. Enter Type (Regular, Express, SameDay) (or leave empty): ");
                    newTypeInput = Console.ReadLine();

                    if (string.IsNullOrEmpty(newTypeInput)) break;
                }
                // Update if valid
                if (!string.IsNullOrEmpty(newTypeInput))
                    updatedOrder = updatedOrder with { TypeOfOrder = newType };
            }

            Console.Write($"Enter new Address (current: {oldOrder.Address}): ");
            string? newAddress = Console.ReadLine();

            // Update Address if provided
            if (!string.IsNullOrEmpty(newAddress))
                updatedOrder = updatedOrder with { Address = newAddress! };

            Console.Write($"Enter new Latitude (current: {oldOrder.Latitude}): ");
            string? newLatInput = Console.ReadLine();

            // Validate and update Latitude
            if (!string.IsNullOrEmpty(newLatInput))
            {
                double newLat;

                // Validate input
                while (!double.TryParse(newLatInput, out newLat))
                {
                    Console.Write("Invalid number. Enter Latitude (or leave empty): ");
                    newLatInput = Console.ReadLine();
                    if (string.IsNullOrEmpty(newLatInput)) break;
                }

                // Update if valid
                if (!string.IsNullOrEmpty(newLatInput))
                    updatedOrder = updatedOrder with { Latitude = newLat };
            }

            Console.Write($"Enter new Longitude (current: {oldOrder.Longitude}): ");
            string? newLonInput = Console.ReadLine();

            // Validate and update Longitude
            if (!string.IsNullOrEmpty(newLonInput))
            {
                double newLon;

                // Validate input
                while (!double.TryParse(newLonInput, out newLon))
                {
                    Console.Write("Invalid number. Enter Longitude (or leave empty): ");
                    newLonInput = Console.ReadLine();
                    if (string.IsNullOrEmpty(newLonInput)) break;
                }

                // Update if valid
                if (!string.IsNullOrEmpty(newLonInput))
                    updatedOrder = updatedOrder with { Longitude = newLon };
            }

            Console.Write($"Enter new Customer Name (current: {oldOrder.CustomerName}): ");
            string? newName = Console.ReadLine();

            // Update Customer Name if provided
            if (!string.IsNullOrEmpty(newName))
                updatedOrder = updatedOrder with { CustomerName = newName! };

            Console.Write($"Enter new Customer Phone (current: {oldOrder.CustomerPhone}): ");
            string? newPhone = Console.ReadLine();

            // Update Customer Phone if provided
            if (!string.IsNullOrEmpty(newPhone))
                updatedOrder = updatedOrder with { CustomerPhone = newPhone! };

            Console.Write($"Enter new Package Details (current: {oldOrder.PackageDetails}): ");
            string? newDetails = Console.ReadLine();

            // Update Package Details if provided
            if (newDetails != null) 
                updatedOrder = updatedOrder with { PackageDetails = newDetails };

            Console.Write($"Enter new Description (current: {oldOrder.Description}): ");
            string? newDesc = Console.ReadLine();

            // Update Description if provided
            if (newDesc != null)
                updatedOrder = updatedOrder with { Description = newDesc };

            // Call to DAL to update the order
            s_dal!.Order.Update(updatedOrder);

            Console.WriteLine($"Successfully updated Order {id}");
        }
        catch (DalNullValueException ex)
        {
            Console.WriteLine($"Error updating order: {ex.Message}");
        }
        catch (DalDoesNotExistException ex)
        {
            Console.WriteLine($"Error updating order: {ex.Message}");
        }
    }

    /// <summary>
    /// helper function to delete an order by ID
    /// </summary>
    private static void DeleteOrder()
    {
        try
        {
            Console.Write("Enter Order ID to delete: ");
            int id;

            // Validate input
            while (!int.TryParse(Console.ReadLine(), out id))
                Console.Write("Invalid input. Please enter a valid number for ID: ");

            // Call to DAL to delete the order
            s_dal!.Order.Delete(id);

            Console.WriteLine($"Successfully deleted Order {id}");
        }
        catch (DalNullValueException ex)
        {
            Console.WriteLine($"Error deleting order: {ex.Message}");
        }
        catch (DalDoesNotExistException ex)
        {
            Console.WriteLine($"Error deleting order: {ex.Message}");
        }
    }

    /// <summary>
    /// helper function to delete all orders
    /// </summary>
    private static void DeleteAllOrders()
    {
        try
        {
            // Call to DAL to delete all orders
            s_dal!.Order.DeleteAll();

            Console.WriteLine("Successfully deleted all orders.");
        }

        catch (DalDoesNotExistException ex)
        {
            Console.WriteLine($"Error deleting all deliveries: {ex.Message}");
        }

    }

    /// <summary>
    /// helper function to display the order menu
    /// </summary>
    private static void OrderMenu()
    {
        bool exit = false;

        // Loop until the user chooses to exit
        while (!exit)
        {
            CrudMenuOptions choice = ShowCrudMenu("Order");

            // Handle the user's choice
            switch (choice)
            {
                case CrudMenuOptions.Exit:
                    exit = true;
                    break;
                case CrudMenuOptions.Create:
                    AddOrder(); 
                    break;
                case CrudMenuOptions.Read:
                    GetOrder(); 
                    break;
                case CrudMenuOptions.ReadAll:
                    ListAllOrders(); 
                    break;
                case CrudMenuOptions.Update:
                    UpdateOrder(); 
                    break;
                case CrudMenuOptions.Delete:
                    DeleteOrder(); 
                    break;
                case CrudMenuOptions.DeleteAll:
                    DeleteAllOrders(); 
                    break;
            }
        }
    }


    /// <summary>
    /// helper function to add a new delivery (linking order to courier)
    /// </summary>
    private static void AddDelivery()
    {
        try
        {
            Console.Write("Enter Order ID to assign: ");
            int orderId;

            // Validate input
            while (!int.TryParse(Console.ReadLine(), out orderId))
                Console.Write("Invalid input. Please enter a valid number for Order ID: ");

            Console.Write("Enter Courier ID to assign: ");
            int courierId;
            while (!int.TryParse(Console.ReadLine(), out courierId))
                Console.Write("Invalid input. Please enter a valid number for Courier ID: ");

            Console.Write("Enter Order Type (Regular, Express, SameDay): ");
            DO.OrderType typeOfOrder;
            while (!Enum.TryParse(Console.ReadLine(), true, out typeOfOrder))
                Console.Write("Invalid type. Please enter (Regular, Express, SameDay): ");

            int id = 0; // ID will be set by DAL
            DateTime deliveryStartTime = s_dal!.Config.Clock; // Delivery start time

            // Call to DAL to create the delivery
            s_dal!.Delivery.Create(new(id, orderId, courierId, typeOfOrder, deliveryStartTime, null, null, null));

            Console.WriteLine($"Successfully created new delivery, assigning Order {orderId} to Courier {courierId}");
        }
        catch (DalAlreadyExistsException ex)
        {
            Console.WriteLine($"Error adding delivery: {ex.Message}");
        }
    }

    /// <summary>
    /// helper function to get and display a delivery by ID
    /// </summary>
    private static void GetDelivery()
    {
        try
        {
            Console.Write("Enter Delivery ID to get: ");
            int id;
            while (!int.TryParse(Console.ReadLine(), out id))
                Console.Write("Invalid input. Please enter a valid number for ID: ");

            // Call to DAL
            DO.Delivery? delivery = s_dal!.Delivery.Read(id);

            // Check if delivery was found
            if (delivery == null)
            {
                Console.WriteLine($"Delivery with ID={id} not found.");
                return;
            }

            Console.WriteLine(delivery);
        }
        catch (DalNullValueException ex)
        {
            Console.WriteLine($"Error getting delivery: {ex.Message}");
        }
        catch (DalDoesNotExistException ex)
        {
            Console.WriteLine($"Error getting delivery: {ex.Message}");
        }
    }

    /// <summary>
    /// helper function to list all deliveries
    /// </summary>
    private static void ListAllDeliveries()
    {
        try
        {
            List<DO.Delivery> deliveries = s_dal!.Delivery.ReadAll().ToList();

            if (deliveries.Count == 0)
            {
                Console.WriteLine("No deliveries found in the database.");
                return;
            }

            foreach (var delivery in deliveries)
            {
                Console.WriteLine(delivery);
            }
        }
        catch (DalNullValueException ex)
        {
            Console.WriteLine($"Error listing deliveries: {ex.Message}");
        }
        catch (DalDoesNotExistException ex)
        {
            Console.WriteLine($"Error listing deliveries: {ex.Message}");
        }

    }

    /// <summary>
    /// helper function to update an existing delivery
    /// </summary>
    private static void UpdateDelivery()
    {
        try
        {
            // Get ID and read the object
            Console.Write("Enter Delivery ID to update: ");
            int id;
            while (!int.TryParse(Console.ReadLine(), out id))
                Console.Write("Invalid input. Please enter a valid number for ID: ");

            DO.Delivery? oldDelivery = s_dal!.Delivery.Read(id);
            if (oldDelivery == null)
            {
                Console.WriteLine($"Delivery with ID={id} not found.");
                return;
            }

            Console.WriteLine("Current values:");
            Console.WriteLine(oldDelivery);

            // Get new values
            DO.Delivery updatedDelivery = oldDelivery with { };

            Console.Write($"Enter new Actual Distance (current: {oldDelivery.ActualDistance}): ");
            string? newDistInput = Console.ReadLine();

            // Validate and update Actual Distance
            if (!string.IsNullOrEmpty(newDistInput))
            {
                double newDist;

                // Validate input
                while (!double.TryParse(newDistInput, out newDist))
                {
                    Console.Write("Invalid number. Enter Distance (or leave empty): ");
                    newDistInput = Console.ReadLine();
                    if (string.IsNullOrEmpty(newDistInput)) break;
                }

                // Update if valid
                if (!string.IsNullOrEmpty(newDistInput))
                    updatedDelivery = updatedDelivery with { ActualDistance = newDist };
            }

            Console.Write($"Enter new Order End Status (current: {oldDelivery.OrderClosedStatus}): ");
            string? newStatusInput = Console.ReadLine();

            // Validate and update Order End Status
            if (!string.IsNullOrEmpty(newStatusInput))
            {
                DO.OrderEndStatus newStatus;
                // Validate input
                while (!Enum.TryParse(newStatusInput, true, out newStatus))
                {
                    Console.Write("Invalid. Enter Status (Delivered, Refused, etc.) (or leave empty): ");
                    newStatusInput = Console.ReadLine();
                    if (string.IsNullOrEmpty(newStatusInput)) break;
                }
                if (!string.IsNullOrEmpty(newStatusInput))
                    updatedDelivery = updatedDelivery with { OrderClosedStatus = newStatus };
            }

            // Get Delivery End Time. We're not taking from the system clock,
            // but allowing the user to enter a time to test the DAL.
            Console.Write($"Enter new Delivery End Time (current: {oldDelivery.DeliveryEndTime}): ");
            string? newEndTimeInput = Console.ReadLine();

            // Validate and update Delivery End Time
            if (!string.IsNullOrEmpty(newEndTimeInput))
            {
                DateTime newEndTime;

                // Validate input
                while (!DateTime.TryParse(newEndTimeInput, out newEndTime))
                {
                    Console.Write("Invalid. Enter time (e.g., 'dd/mm/yyyy hh:mm') (or leave empty): ");
                    newEndTimeInput = Console.ReadLine();
                    if (string.IsNullOrEmpty(newEndTimeInput)) break;
                }
                if (!string.IsNullOrEmpty(newEndTimeInput))
                    updatedDelivery = updatedDelivery with { DeliveryEndTime = newEndTime };
            }

            // Call DAL
            s_dal!.Delivery.Update(updatedDelivery);

            Console.WriteLine($"Successfully updated Delivery {id}");
        }
        catch (DalNullValueException ex)
        {
            Console.WriteLine($"Error updating delivery: {ex.Message}");
        }
        catch (DalDoesNotExistException ex)
        {
            Console.WriteLine($"Error updating delivery: {ex.Message}");
        }
    }

    /// <summary>
    /// helper function to delete a delivery by ID
    /// </summary>
    private static void DeleteDelivery()
    {
        try
        {
            Console.Write("Enter Delivery ID to delete: ");
            int id;
            while (!int.TryParse(Console.ReadLine(), out id))
                Console.Write("Invalid input. Please enter a valid number for ID: ");

            // Call the delete function from the DAL
            s_dal!.Delivery.Delete(id);

            Console.WriteLine($"Successfully deleted Delivery {id}");
        }
        catch (DalNullValueException ex)
        {
            Console.WriteLine($"Error deleting delivery: {ex.Message}");
        }
        catch (DalDoesNotExistException ex)
        {
            Console.WriteLine($"Error deleting delivery: {ex.Message}");
        }
    }

    /// <summary>
    /// helper function to delete all deliveries
    /// </summary>
    private static void DeleteAllDeliveries()
    {
        try
        {
            // Call the delete function from the DAL
            s_dal!.Delivery.DeleteAll();

            Console.WriteLine("Successfully deleted all deliveries.");
        }
        catch (DalNullValueException ex)
        {
            Console.WriteLine($"Error deleting all deliveries: {ex.Message}");
        }
        catch (DalDoesNotExistException ex)
        {
            Console.WriteLine($"Error deleting all deliveries: {ex.Message}");
        }
    }

    /// <summary>
    /// helper function to manage the delivery submenu
    /// </summary>
    private static void DeliveryMenu()
    {
        bool exit = false;

        // Loop until the user chooses to exit
        while (!exit)
        {
            CrudMenuOptions choice = ShowCrudMenu("Delivery");

            switch (choice)
            {
                case CrudMenuOptions.Exit:
                    exit = true;
                    break;
                case CrudMenuOptions.Create:
                    AddDelivery(); 
                    break;
                case CrudMenuOptions.Read:
                    GetDelivery(); 
                    break;
                case CrudMenuOptions.ReadAll:
                    ListAllDeliveries(); 
                    break;
                case CrudMenuOptions.Update:
                    UpdateDelivery(); 
                    break;
                case CrudMenuOptions.Delete:
                    DeleteDelivery(); 
                    break;
                case CrudMenuOptions.DeleteAll:
                    DeleteAllDeliveries(); 
                    break;
            }
        }
    }


    /// <summary>
    /// shows the current system clock
    /// </summary>
    private static void ShowClock()
    {
        Console.WriteLine($"Current System Clock: {s_dal!.Config.Clock}");
    }

    /// <summary>
    /// Advances the system clock by a specified time span.
    /// </summary>
    /// <param name="span">The time span to add (minutes, hours, days...)</param>
    private static void AdvanceClock(TimeSpan span)
    {
        // 1. Get the old time
        DateTime oldTime = s_dal!.Config.Clock;
        // 2. Calculate the new time
        DateTime newTime = oldTime.Add(span);
        // 3. Update the time in the DAL
        s_dal!.Config.Clock = newTime;

        Console.WriteLine($"System clock advanced from {oldTime} to {newTime}");
    }

    /// <summary>
    /// Resets all system settings to their default values.
    /// </summary>
    private static void ResetConfig()
    {
        s_dal!.Config.Reset();
        Console.WriteLine("Configuration has been reset to default values.");
    }

    /// <summary>
    /// helper function to show the value of a variable from the Config
    /// </summary>
    private static void ShowVariable()
    {
        try
        {
            Console.WriteLine("Which config variable do you want to show?");
            Console.WriteLine("1: Admin ID");
            Console.WriteLine("2: Company Address");
            Console.WriteLine("3: Max Delivery Distance");
            Console.WriteLine("4: Vehicle Speed");
            Console.WriteLine("5: Motorcycle Speed");
            Console.WriteLine("6: Bicycle Speed");
            Console.WriteLine("7: By-Foot Speed");
            Console.WriteLine("8: Max Delivery Time Range");
            Console.WriteLine("9: Risk Time Range");
            Console.WriteLine("10: Inactivity Time Range");
            Console.Write("Enter your choice: ");

            string? choice = Console.ReadLine();

            Console.Write("Current Value: ");

            // Display the selected variable
            switch (choice)
            {
                case "1":
                    Console.WriteLine(s_dal!.Config.AdminId);
                    break;
                case "2":
                    Console.WriteLine(s_dal!.Config.CompenyAddress ?? "null");
                    break;
                case "3":
                    Console.WriteLine(s_dal!.Config.DeliveryMaxDistance?.ToString() ?? "null");
                    break;
                case "4":
                    Console.WriteLine(s_dal!.Config.AverageVehicleSpeedKmH);
                    break;
                case "5":
                    Console.WriteLine(s_dal!.Config.AverageMotorcycleSpeedKmH);
                    break;
                case "6":
                    Console.WriteLine(s_dal!.Config.AverageBicycleSpeedKmH);
                    break;
                case "7":
                    Console.WriteLine(s_dal!.Config.AverageByFootSpeedKmH);
                    break;
                case "8":
                    Console.WriteLine(s_dal!.Config.MaxDeliveryRange);
                    break;
                case "9":
                    Console.WriteLine(s_dal!.Config.RiskRange);
                    break;
                case "10":
                    Console.WriteLine(s_dal!.Config!.InactivityTimeRange);
                    break;
                default:
                    Console.WriteLine("Invalid choice.");
                    break;
            }
        }
        catch (DalDoesNotExistException ex)
        {
            Console.WriteLine($"Error showing variable: {ex.Message}");
        }
        catch (DalNullValueException ex)
        {
            Console.WriteLine($"Error showing variable: {ex.Message}");
        }
    }

    /// <summary>
    /// helper function to update the value of a variable in the Config
    /// </summary>
    private static void UpdateVariable()
    {
        try
        {
            Console.WriteLine("Which config variable do you want to update?");
            Console.WriteLine("1: Admin ID");
            Console.WriteLine("2: Company Address");
            Console.WriteLine("3: Max Delivery Distance");
            Console.WriteLine("4: Vehicle Speed");
            Console.WriteLine("5: Motorcycle Speed");
            Console.WriteLine("6: Bicycle Speed");
            Console.WriteLine("7: By-Foot Speed");
            Console.WriteLine("8: Max Delivery Time Range");
            Console.WriteLine("9: Risk Time Range");
            Console.WriteLine("10: Inactivity Time Range");
            Console.Write("Enter your choice: ");

            string? choice = Console.ReadLine();
            string? input;

            // Update the selected variable
            switch (choice)
            {
                case "1": // Admin ID (int)
                    Console.Write($"Enter new Admin ID (current: {s_dal!.Config.AdminId}): ");
                    input = Console.ReadLine();
                    if (!string.IsNullOrEmpty(input) && int.TryParse(input, out int newAdminId))
                    {
                        s_dal!.Config.AdminId = newAdminId;
                        Console.WriteLine("Admin ID updated.");
                    }
                    else if (!string.IsNullOrEmpty(input))
                    {
                        Console.WriteLine("Invalid number.");
                    }
                    break;

                case "2": // Company Address (string)
                    Console.Write($"Enter new Company Address (current: {s_dal!.Config.CompenyAddress ?? "null"}): ");
                    input = Console.ReadLine();
                    if (input != null)
                    {
                        s_dal!.Config.CompenyAddress = input;
                        Console.WriteLine("Company Address updated.");
                    }
                    break;

                case "3": // Max Delivery Distance (double?)
                    Console.Write($"Enter new Max Distance (current: {s_dal!.Config.DeliveryMaxDistance?.ToString() ?? "null"}): ");
                    input = Console.ReadLine();
                    if (string.IsNullOrEmpty(input))
                    {
                        s_dal!.Config.DeliveryMaxDistance = null;
                        Console.WriteLine("Max Distance set to null.");
                    }
                    else if (double.TryParse(input, out double newMaxDist))
                    {
                        s_dal!.Config.DeliveryMaxDistance = newMaxDist;
                        Console.WriteLine("Max Distance updated.");
                    }
                    else
                    {
                        Console.WriteLine("Invalid number.");
                    }
                    break;

                case "4": // Vehicle Speed (double)
                    Console.Write($"Enter new Vehicle Speed (current: {s_dal!.Config.AverageVehicleSpeedKmH}): ");
                    input = Console.ReadLine();
                    if (!string.IsNullOrEmpty(input) && double.TryParse(input, out double newSpeedV))
                    {
                        s_dal!.Config.AverageVehicleSpeedKmH = newSpeedV;
                        Console.WriteLine("Vehicle Speed updated.");
                    }
                    else if (!string.IsNullOrEmpty(input)) { Console.WriteLine("Invalid number."); }
                    break;

                case "5": // Motorcycle Speed (double)
                    Console.Write($"Enter new Motorcycle Speed (current: {s_dal!.Config.AverageMotorcycleSpeedKmH}): ");
                    input = Console.ReadLine();
                    if (!string.IsNullOrEmpty(input) && double.TryParse(input, out double newSpeedM))
                    {
                        s_dal!.Config.AverageMotorcycleSpeedKmH = newSpeedM;
                        Console.WriteLine("Motorcycle Speed updated.");
                    }
                    else if (!string.IsNullOrEmpty(input)) { Console.WriteLine("Invalid number."); }
                    break;

                case "6": // Bicycle Speed (double)
                    Console.Write($"Enter new Bicycle Speed (current: {s_dal!.Config.AverageBicycleSpeedKmH}): ");
                    input = Console.ReadLine();
                    if (!string.IsNullOrEmpty(input) && double.TryParse(input, out double newSpeedB))
                    {
                        s_dal!.Config.AverageBicycleSpeedKmH = newSpeedB;
                        Console.WriteLine("Bicycle Speed updated.");
                    }
                    else if (!string.IsNullOrEmpty(input)) { Console.WriteLine("Invalid number."); }
                    break;

                case "7": // By-Foot Speed (double)
                    Console.Write($"Enter new By-Foot Speed (current: {s_dal!.Config.AverageByFootSpeedKmH}): ");
                    input = Console.ReadLine();
                    if (!string.IsNullOrEmpty(input) && double.TryParse(input, out double newSpeedF))
                    {
                        s_dal!.Config.AverageByFootSpeedKmH = newSpeedF;
                        Console.WriteLine("By-Foot Speed updated.");
                    }
                    else if (!string.IsNullOrEmpty(input)) { Console.WriteLine("Invalid number."); }
                    break;

                case "8": // Max Delivery Time Range (TimeSpan)
                    Console.Write($"Enter new Max Delivery Range (in minutes) (current: {s_dal!.Config.MaxDeliveryRange.TotalMinutes}): ");
                    input = Console.ReadLine();
                    if (!string.IsNullOrEmpty(input) && double.TryParse(input, out double newMinutesM))
                    {
                        s_dal!.Config.MaxDeliveryRange = TimeSpan.FromMinutes(newMinutesM);
                        Console.WriteLine("Max Delivery Range updated.");
                    }
                    else if (!string.IsNullOrEmpty(input)) { Console.WriteLine("Invalid number."); }
                    break;

                case "9": // Risk Time Range (TimeSpan)
                    Console.Write($"Enter new Risk Range (in minutes) (current: {s_dal!.Config.RiskRange.TotalMinutes}): ");
                    input = Console.ReadLine();
                    if (!string.IsNullOrEmpty(input) && double.TryParse(input, out double newMinutesR))
                    {
                        s_dal!.Config.RiskRange = TimeSpan.FromMinutes(newMinutesR);
                        Console.WriteLine("Risk Range updated.");
                    }
                    else if (!string.IsNullOrEmpty(input)) { Console.WriteLine("Invalid number."); }
                    break;

                case "10": // Inactivity Time Range (TimeSpan)
                    Console.Write($"Enter new Inactivity Range (in minutes) (current: {s_dal!.Config.InactivityTimeRange.TotalMinutes}): ");
                    input = Console.ReadLine();
                    if (!string.IsNullOrEmpty(input) && double.TryParse(input, out double newMinutesI))
                    {
                        s_dal!.Config.InactivityTimeRange = TimeSpan.FromMinutes(newMinutesI);
                        Console.WriteLine("Inactivity Range updated.");
                    }
                    else if (!string.IsNullOrEmpty(input)) { Console.WriteLine("Invalid number."); }
                    break;

                default:
                    Console.WriteLine("Invalid choice.");
                    break;
            }
        }
        catch (DalDoesNotExistException ex)
        {
            Console.WriteLine($"Error updating variable: {ex.Message}");
        }
        catch (DalNullValueException ex)
        {
            Console.WriteLine($"Error updating variable: {ex.Message}");
        }
    }
    
    /// <summary>
    /// helper function to manage the configuration settings menu
    /// </summary>
    private static void ConfigMenu()
    {
        bool exit = false;
        while (!exit)
        {
            ConfigMenuOptions choice = ShowConfigMenu();

            try
            {
                // Handle the user's choice
                switch (choice)
                {
                    case ConfigMenuOptions.Exit:
                        exit = true;
                        break;
                    case ConfigMenuOptions.AddMinute:
                        AdvanceClock(TimeSpan.FromMinutes(1));
                        break;
                    case ConfigMenuOptions.AddHour:
                        AdvanceClock(TimeSpan.FromHours(1));
                        break;
                    case ConfigMenuOptions.AddDay:
                        AdvanceClock(TimeSpan.FromDays(1));
                        break;
                    case ConfigMenuOptions.AddWeek:
                        AdvanceClock(TimeSpan.FromDays(7));
                        break;
                    case ConfigMenuOptions.AddMonth:
                        AdvanceClock(TimeSpan.FromDays(30));
                        break;
                    case ConfigMenuOptions.AddYear:
                        AdvanceClock(TimeSpan.FromDays(365));
                        break;
                    case ConfigMenuOptions.ShowCurrentClock:
                        ShowClock();
                        break;
                    case ConfigMenuOptions.UpdateVariable:
                        UpdateVariable();
                        break;
                    case ConfigMenuOptions.ShowVariable:
                        ShowVariable();
                        break;
                    case ConfigMenuOptions.ResetAllConfig:
                        ResetConfig();
                        break;
                }
            }
            catch (DalNullValueException ex)
            {
                Console.WriteLine($"Error in config menu: {ex.Message}");
            }
            catch (DalDoesNotExistException ex)
            {
                Console.WriteLine($"Error in config menu: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// main entry point of the program
    /// </summary>
    /// <param name="args"></param>
    static void Main(string[] args)
    {
        // Activate initial setup (Chapter 10)
        try
        {
            Console.WriteLine("Initializing data...");
            // Call the function from Chapter 10 with the instances we created

            Initialization.Do();
            Console.WriteLine("Data initialized successfully.");
        }
        catch (DalNullValueException ex)
        {
            Console.WriteLine($"Critical error during initialization: {ex.Message}");
            Console.WriteLine("Press Enter to exit.");
            Console.ReadLine();
            return; // Exit the program if initialization fails
        }

        // Main menu loop
        bool exit = false;
        while (!exit)
        {
            // Call our helper function
            MainMenuOptions choice = ShowMainMenu();

            try
            {
                // Handle the user's choice
                switch (choice)
                {
                    case MainMenuOptions.Exit:
                        exit = true;
                        Console.WriteLine("Exiting program. Goodbye!");
                        break;
                    case MainMenuOptions.Courier:
                        CourierMenu(); // Call the helper function for couriers
                        break;
                    case MainMenuOptions.Order:
                        OrderMenu(); // Call the helper function for orders
                        break;
                    case MainMenuOptions.Delivery:
                        DeliveryMenu(); // Call the helper function for deliveries
                        break;
                    case MainMenuOptions.Config:
                        ConfigMenu(); // Call the helper function for configuration
                        break;
                    case MainMenuOptions.InitializeData:
                        // Option to re-run initialization
                        Console.WriteLine("Re-initializing data (Reset + Create)...");
                        Initialization.Do();
                        Console.WriteLine("Data re-initialized successfully.");
                        break;
                    case MainMenuOptions.ResetData:
                        Console.WriteLine("Resetting all data...");
                        s_dal!.Courier.DeleteAll();
                        s_dal!.Order.DeleteAll();
                        s_dal!.Delivery.DeleteAll();
                        s_dal!.Config.Reset();
                        Console.WriteLine("All data reset.");
                        break;
                    case MainMenuOptions.ListAllData:
                        // Option to list all data at once
                        Console.WriteLine("\n--- LISTING ALL DATA ---");
                        Console.WriteLine("\n-- Couriers --");
                        ListAllCouriers();
                        Console.WriteLine("\n-- Orders --");
                        ListAllOrders();
                        Console.WriteLine("\n-- Deliveries --");
                        ListAllDeliveries();
                        Console.WriteLine("\n--- END OF LIST ---");
                        break;
                }
            }
            catch (DalNullValueException ex)
            {
                // General exception handling for all DAL layers
                Console.WriteLine($"An error occurred: {ex.Message}");
                Console.WriteLine("Returning to main menu. Press Enter to continue...");
                Console.ReadLine(); // Wait for the user to acknowledge the error
            }
        }
    }
}