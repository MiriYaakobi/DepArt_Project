/// <summary>
/// Main entry point for the application.
/// </summary>
using Dal;
using DalApi;
using DO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using static DalTest.Program;

namespace DalTest;

internal class Program
{
    //creating the DAL objects
    private static ICourier? s_dalCourier = new CourierImplementation();
    private static IOrder? s_dalOrder = new OrderImplementation();
    private static IDelivery? s_dalDelivery = new DeliveryImplementation();
    private static IConfig? s_dalConfig = new ConfigImplementation();

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
            DateTime startWorkTime = s_dalConfig!.Clock;

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
            DO.Courier newCourier = new DO.Courier
            (
                Id: id,
                Name: name!,
                Phone: phone!,
                Email: email!,
                Password: password!,
                IsActive: isActive,
                TypeOfDelivery: typeOfDelivery,
                StartWorkTime: startWorkTime,
                MaxDist: maxDist
            );

            s_dalCourier!.Create(newCourier);

            Console.WriteLine($"Successfully added Courier {id} - {name}");
        }
        // Catch any exceptions that occur during the process
        catch (Exception ex)
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
            DO.Courier? courier = s_dalCourier!.Read(id);

            // Check if courier was found
            if (courier == null)
            {
                Console.WriteLine($"Courier with ID={id} not found.");
                return;
            }

            Console.WriteLine(courier); 
        }
        // Catch any exceptions that occur during the process
        catch (Exception ex)
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
            List<DO.Courier> couriers = s_dalCourier!.ReadAll();

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
        // Catch any exceptions that occur during the process
        catch (Exception ex)
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
            DO.Courier? oldCourier = s_dalCourier!.Read(id);
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

            Console.Write($"Enter new Max Delivery Distance (current: {oldCourier.MaxDist}): ");
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
                    updatedCourier = updatedCourier with { MaxDist = newMaxDist };
            }

            s_dalCourier!.Update(updatedCourier);

            Console.WriteLine($"Successfully updated Courier {id}");
        }
        // Catch any exceptions that occur during the process
        catch (Exception ex)
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
            s_dalCourier!.Delete(id);

            Console.WriteLine($"Successfully deleted Courier {id}");
        }
        // Catch any exceptions that occur during the process
        catch (Exception ex) 
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
            s_dalCourier!.DeleteAll();

            Console.WriteLine("Successfully deleted all couriers.");
        }
        catch (Exception ex)
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
            DateTime orderOpeningTime = s_dalConfig!.Clock;

            Console.Write("Enter Package Details (optional, press Enter to skip): ");
            string? packageDetails = Console.ReadLine();

            Console.Write("Enter Description (optional, press Enter to skip): ");
            string? description = Console.ReadLine();

            // Create the new order object
            DO.Order newOrder = new DO.Order
            (
                Id: id,
                TypeOfOrder: typeOfOrder,
                Address: address!,
                Latitude: latitude,
                Longitude: longitude,
                CustomerName: customerName!,
                CustomerPhone: customerPhone!,
                OrderOpeningTime: orderOpeningTime,
                PackageDetails: packageDetails,
                Description: description
            );

            s_dalOrder!.Create(newOrder);

            Console.WriteLine($"Successfully added new order for {customerName}");
        }
        // Catch any exceptions that occur during the process
        catch (Exception ex)
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
            DO.Order? order = s_dalOrder!.Read(id);

            // Check if order was found
            if (order == null)
            {
                Console.WriteLine($"Order with ID={id} not found.");
                return;
            }

            Console.WriteLine(order); 
        }
        // Catch any exceptions that occur during the process
        catch (Exception ex) 
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
            List<DO.Order> orders = s_dalOrder!.ReadAll();

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
        // Catch any exceptions that occur during the process
        catch (Exception ex) 
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
            DO.Order? oldOrder = s_dalOrder!.Read(id);

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
            s_dalOrder!.Update(updatedOrder);

            Console.WriteLine($"Successfully updated Order {id}");
        }
        // Catch any exceptions that occur during the process
        catch (Exception ex)
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
            s_dalOrder!.Delete(id);

            Console.WriteLine($"Successfully deleted Order {id}");
        }
        // Catch any exceptions that occur during the process
        catch (Exception ex) 
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
            s_dalOrder!.DeleteAll();

            Console.WriteLine("Successfully deleted all orders.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting all orders: {ex.Message}");
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
            DateTime deliveryStartTime = s_dalConfig!.Clock; // Delivery start time

            // Create the object
            DO.Delivery newDelivery = new DO.Delivery
            (
                Id: id,
                OrderId: orderId,
                CourierId: courierId,
                TypeOfOrder: typeOfOrder,
                DeliveryStartTime: deliveryStartTime,
                ActualDistance: null, 
                OrderEndStatus: null, 
                DeliveryEndTime: null  
            );

            // Call to DAL to create the delivery
            s_dalDelivery!.Create(newDelivery);

            Console.WriteLine($"Successfully created new delivery, assigning Order {orderId} to Courier {courierId}");
        }
        // Catch any exceptions that occur during the process
        catch (Exception ex)
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
            DO.Delivery? delivery = s_dalDelivery!.Read(id);

            // Check if delivery was found
            if (delivery == null)
            {
                Console.WriteLine($"Delivery with ID={id} not found.");
                return;
            }

            Console.WriteLine(delivery);
        }
        // Catch any exceptions that occur during the process
        catch (Exception ex)
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
            List<DO.Delivery> deliveries = s_dalDelivery!.ReadAll();

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
        catch (Exception ex)
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

            DO.Delivery? oldDelivery = s_dalDelivery!.Read(id);
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

            Console.Write($"Enter new Order End Status (current: {oldDelivery.OrderEndStatus}): ");
            string? newStatusInput = Console.ReadLine();

            // Validate and update Order End Status
            if (!string.IsNullOrEmpty(newStatusInput))
            {
                DO.OrderStatus newStatus;
                // Validate input
                while (!Enum.TryParse(newStatusInput, true, out newStatus))
                {
                    Console.Write("Invalid. Enter Status (Delivered, Refused, etc.) (or leave empty): ");
                    newStatusInput = Console.ReadLine();
                    if (string.IsNullOrEmpty(newStatusInput)) break;
                }
                if (!string.IsNullOrEmpty(newStatusInput))
                    updatedDelivery = updatedDelivery with { OrderEndStatus = newStatus };
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
            s_dalDelivery!.Update(updatedDelivery);

            Console.WriteLine($"Successfully updated Delivery {id}");
        }
        // Catch any exceptions
        catch (Exception ex)
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
            s_dalDelivery!.Delete(id);

            Console.WriteLine($"Successfully deleted Delivery {id}");
        }
        catch (Exception ex)
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
            s_dalDelivery!.DeleteAll();

            Console.WriteLine("Successfully deleted all deliveries.");
        }
        catch (Exception ex)
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
        Console.WriteLine($"Current System Clock: {s_dalConfig!.Clock}");
    }

    /// <summary>
    /// Advances the system clock by a specified time span.
    /// </summary>
    /// <param name="span">The time span to add (minutes, hours, days...)</param>
    private static void AdvanceClock(TimeSpan span)
    {
        // 1. Get the old time
        DateTime oldTime = s_dalConfig!.Clock;
        // 2. Calculate the new time
        DateTime newTime = oldTime.Add(span);
        // 3. Update the time in the DAL
        s_dalConfig.Clock = newTime;

        Console.WriteLine($"System clock advanced from {oldTime} to {newTime}");
    }

    /// <summary>
    /// Resets all system settings to their default values.
    /// </summary>
    private static void ResetConfig()
    {
        s_dalConfig!.Reset();
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
                    Console.WriteLine(s_dalConfig!.AdminId);
                    break;
                case "2":
                    Console.WriteLine(s_dalConfig!.CompenyAddress ?? "null");
                    break;
                case "3":
                    Console.WriteLine(s_dalConfig!.DeliveryMaxDistance?.ToString() ?? "null");
                    break;
                case "4":
                    Console.WriteLine(s_dalConfig!.AverageVehicleSpeedKmH);
                    break;
                case "5":
                    Console.WriteLine(s_dalConfig!.AverageMotorcycleSpeedKmH);
                    break;
                case "6":
                    Console.WriteLine(s_dalConfig!.AverageBicycleSpeedKmH);
                    break;
                case "7":
                    Console.WriteLine(s_dalConfig!.AverageByFootSpeedKmH);
                    break;
                case "8":
                    Console.WriteLine(s_dalConfig!.MaxDeliveryRange);
                    break;
                case "9":
                    Console.WriteLine(s_dalConfig!.RiskRange);
                    break;
                case "10":
                    Console.WriteLine(s_dalConfig!.InactivityTimeRange);
                    break;
                default:
                    Console.WriteLine("Invalid choice.");
                    break;
            }
        }
        catch (Exception ex)
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
                    Console.Write($"Enter new Admin ID (current: {s_dalConfig!.AdminId}): ");
                    input = Console.ReadLine();
                    if (!string.IsNullOrEmpty(input) && int.TryParse(input, out int newAdminId))
                    {
                        s_dalConfig!.AdminId = newAdminId;
                        Console.WriteLine("Admin ID updated.");
                    }
                    else if (!string.IsNullOrEmpty(input))
                    {
                        Console.WriteLine("Invalid number.");
                    }
                    break;

                case "2": // Company Address (string)
                    Console.Write($"Enter new Company Address (current: {s_dalConfig!.CompenyAddress ?? "null"}): ");
                    input = Console.ReadLine();
                    if (input != null)
                    {
                        s_dalConfig!.CompenyAddress = input;
                        Console.WriteLine("Company Address updated.");
                    }
                    break;

                case "3": // Max Delivery Distance (double?)
                    Console.Write($"Enter new Max Distance (current: {s_dalConfig!.DeliveryMaxDistance?.ToString() ?? "null"}): ");
                    input = Console.ReadLine();
                    if (string.IsNullOrEmpty(input))
                    {
                        s_dalConfig!.DeliveryMaxDistance = null;
                        Console.WriteLine("Max Distance set to null.");
                    }
                    else if (double.TryParse(input, out double newMaxDist))
                    {
                        s_dalConfig!.DeliveryMaxDistance = newMaxDist;
                        Console.WriteLine("Max Distance updated.");
                    }
                    else
                    {
                        Console.WriteLine("Invalid number.");
                    }
                    break;

                case "4": // Vehicle Speed (double)
                    Console.Write($"Enter new Vehicle Speed (current: {s_dalConfig!.AverageVehicleSpeedKmH}): ");
                    input = Console.ReadLine();
                    if (!string.IsNullOrEmpty(input) && double.TryParse(input, out double newSpeedV))
                    {
                        s_dalConfig!.AverageVehicleSpeedKmH = newSpeedV;
                        Console.WriteLine("Vehicle Speed updated.");
                    }
                    else if (!string.IsNullOrEmpty(input)) { Console.WriteLine("Invalid number."); }
                    break;

                case "5": // Motorcycle Speed (double)
                    Console.Write($"Enter new Motorcycle Speed (current: {s_dalConfig!.AverageMotorcycleSpeedKmH}): ");
                    input = Console.ReadLine();
                    if (!string.IsNullOrEmpty(input) && double.TryParse(input, out double newSpeedM))
                    {
                        s_dalConfig!.AverageMotorcycleSpeedKmH = newSpeedM;
                        Console.WriteLine("Motorcycle Speed updated.");
                    }
                    else if (!string.IsNullOrEmpty(input)) { Console.WriteLine("Invalid number."); }
                    break;

                case "6": // Bicycle Speed (double)
                    Console.Write($"Enter new Bicycle Speed (current: {s_dalConfig!.AverageBicycleSpeedKmH}): ");
                    input = Console.ReadLine();
                    if (!string.IsNullOrEmpty(input) && double.TryParse(input, out double newSpeedB))
                    {
                        s_dalConfig!.AverageBicycleSpeedKmH = newSpeedB;
                        Console.WriteLine("Bicycle Speed updated.");
                    }
                    else if (!string.IsNullOrEmpty(input)) { Console.WriteLine("Invalid number."); }
                    break;

                case "7": // By-Foot Speed (double)
                    Console.Write($"Enter new By-Foot Speed (current: {s_dalConfig!.AverageByFootSpeedKmH}): ");
                    input = Console.ReadLine();
                    if (!string.IsNullOrEmpty(input) && double.TryParse(input, out double newSpeedF))
                    {
                        s_dalConfig!.AverageByFootSpeedKmH = newSpeedF;
                        Console.WriteLine("By-Foot Speed updated.");
                    }
                    else if (!string.IsNullOrEmpty(input)) { Console.WriteLine("Invalid number."); }
                    break;

                case "8": // Max Delivery Time Range (TimeSpan)
                    Console.Write($"Enter new Max Delivery Range (in minutes) (current: {s_dalConfig!.MaxDeliveryRange.TotalMinutes}): ");
                    input = Console.ReadLine();
                    if (!string.IsNullOrEmpty(input) && double.TryParse(input, out double newMinutesM))
                    {
                        s_dalConfig!.MaxDeliveryRange = TimeSpan.FromMinutes(newMinutesM);
                        Console.WriteLine("Max Delivery Range updated.");
                    }
                    else if (!string.IsNullOrEmpty(input)) { Console.WriteLine("Invalid number."); }
                    break;

                case "9": // Risk Time Range (TimeSpan)
                    Console.Write($"Enter new Risk Range (in minutes) (current: {s_dalConfig!.RiskRange.TotalMinutes}): ");
                    input = Console.ReadLine();
                    if (!string.IsNullOrEmpty(input) && double.TryParse(input, out double newMinutesR))
                    {
                        s_dalConfig!.RiskRange = TimeSpan.FromMinutes(newMinutesR);
                        Console.WriteLine("Risk Range updated.");
                    }
                    else if (!string.IsNullOrEmpty(input)) { Console.WriteLine("Invalid number."); }
                    break;

                case "10": // Inactivity Time Range (TimeSpan)
                    Console.Write($"Enter new Inactivity Range (in minutes) (current: {s_dalConfig!.InactivityTimeRange.TotalMinutes}): ");
                    input = Console.ReadLine();
                    if (!string.IsNullOrEmpty(input) && double.TryParse(input, out double newMinutesI))
                    {
                        s_dalConfig!.InactivityTimeRange = TimeSpan.FromMinutes(newMinutesI);
                        Console.WriteLine("Inactivity Range updated.");
                    }
                    else if (!string.IsNullOrEmpty(input)) { Console.WriteLine("Invalid number."); }
                    break;

                default:
                    Console.WriteLine("Invalid choice.");
                    break;
            }
        }
        catch (Exception ex)
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
            catch (Exception ex)
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

            Initialization.Do(s_dalConfig, s_dalCourier, s_dalOrder, s_dalDelivery);
            Console.WriteLine("Data initialized successfully.");
        }
        catch (Exception ex)
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
                        Initialization.Do(s_dalConfig, s_dalCourier, s_dalOrder, s_dalDelivery);
                        Console.WriteLine("Data re-initialized successfully.");
                        break;
                    case MainMenuOptions.ResetData:
                        Console.WriteLine("Resetting all data...");
                        s_dalCourier!.DeleteAll();
                        s_dalOrder!.DeleteAll();
                        s_dalDelivery!.DeleteAll();
                        s_dalConfig!.Reset();
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
            catch (Exception ex)
            {
                // General exception handling for all DAL layers
                Console.WriteLine($"An error occurred: {ex.Message}");
                Console.WriteLine("Returning to main menu. Press Enter to continue...");
                Console.ReadLine(); // Wait for the user to acknowledge the error
            }
        }
    }
}


/*
OutPut example:

Reset Configuration values and List values...
Initializing Couriers...
Initializing Orders...
Initializing Deliveries...
Initialization done.
Data initialized successfully.

--- MAIN MENU ---
0: Exit
1: Courier Menu
2: Order Menu
3: Delivery Menu
4: Config Menu
5: Initialize Data (Reset and Create)
6: Reset All Data (Clear Lists & Config)
7: List All Data
Enter your choice: 7

--- LISTING ALL DATA ---

-- Couriers --
Courier { Id = 850665684, Name = Noa Levi, Phone = 058-793-6600, Email = noa.levi@gmail.com, Password = noa#levi19, IsActive = True, TypeOfDelivery = ByFoot, StartWorkTime = 05/05/2023 03:29:42, MaxDist = 4 }
Courier { Id = 460694135, Name = Daniel Cohen, Phone = 052-981-5362, Email = daniel.cohen@gmail.com, Password = daniel#cohen07, IsActive = True, TypeOfDelivery = ByFoot, StartWorkTime = 12/01/2025 02:25:34, MaxDist = 2 }
Courier { Id = 216276324, Name = Yael Barak, Phone = 053-895-4896, Email = yael.barak@gmail.com, Password = yael#barak00, IsActive = True, TypeOfDelivery = Motorcycle, StartWorkTime = 24/09/2021 05:55:00, MaxDist = 34 }
Courier { Id = 964395664, Name = Roi Avrahami, Phone = 056-192-4981, Email = roi.avrahami@gmail.com, Password = roi#avrahami15, IsActive = True, TypeOfDelivery = Motorcycle, StartWorkTime = 20/07/2021 02:58:06, MaxDist = 44 }
Courier { Id = 650604000, Name = Maya Friedman, Phone = 055-789-9199, Email = maya.friedman@gmail.com, Password = maya#friedman02, IsActive = True, TypeOfDelivery = Motorcycle, StartWorkTime = 12/08/2022 04:54:53, MaxDist = 35 }
Courier { Id = 961718182, Name = Omri Danino, Phone = 058-426-1492, Email = omri.danino@gmail.com, Password = omri#danino03, IsActive = True, TypeOfDelivery = Motorcycle, StartWorkTime = 09/11/2021 13:47:05, MaxDist = 36 }
Courier { Id = 537015669, Name = Tamar Rosen, Phone = 058-186-5302, Email = tamar.rosen@gmail.com, Password = tamar#rosen02, IsActive = False, TypeOfDelivery = Bicycle, StartWorkTime = 28/12/2020 11:30:45, MaxDist = 8 }
Courier { Id = 615022745, Name = Eitan Gabai, Phone = 057-116-4220, Email = eitan.gabai@gmail.com, Password = eitan#gabai12, IsActive = True, TypeOfDelivery = Motorcycle, StartWorkTime = 01/04/2024 07:59:10, MaxDist = 30 }
Courier { Id = 123711248, Name = Shira Neuman, Phone = 054-910-8630, Email = shira.neuman@gmail.com, Password = shira#neuman06, IsActive = True, TypeOfDelivery = ByFoot, StartWorkTime = 05/05/2025 13:21:50, MaxDist = 2 }
Courier { Id = 534280374, Name = Guy Hershkovitz, Phone = 055-181-3193, Email = guy.hershkovitz@gmail.com, Password = guy#hershkovitz07, IsActive = True, TypeOfDelivery = ByFoot, StartWorkTime = 16/12/2023 01:45:08, MaxDist = 1 }
Courier { Id = 729177417, Name = Alon Zahavi, Phone = 053-389-4064, Email = alon.zahavi@gmail.com, Password = alon#zahavi04, IsActive = True, TypeOfDelivery = ByFoot, StartWorkTime = 26/11/2021 04:18:05, MaxDist = 3 }
Courier { Id = 379579023, Name = Hila Ronen, Phone = 053-986-2956, Email = hila.ronen@gmail.com, Password = hila#ronen07, IsActive = True, TypeOfDelivery = Bicycle, StartWorkTime = 23/10/2022 09:07:38, MaxDist = 10 }
Courier { Id = 914548717, Name = Idan Marciano, Phone = 052-293-9108, Email = idan.marciano@gmail.com, Password = idan#marciano00, IsActive = True, TypeOfDelivery = Motorcycle, StartWorkTime = 21/06/2024 10:33:34, MaxDist = 15 }
Courier { Id = 192256906, Name = Rotem Tzadok, Phone = 054-135-1775, Email = rotem.tzadok@gmail.com, Password = rotem#tzadok20, IsActive = False, TypeOfDelivery = Bicycle, StartWorkTime = 31/05/2021 03:39:16, MaxDist = 8 }
Courier { Id = 825755264, Name = Adi Baruch, Phone = 053-173-6027, Email = adi.baruch@gmail.com, Password = adi#baruch20, IsActive = True, TypeOfDelivery = Car, StartWorkTime = 03/11/2022 11:50:08, MaxDist = 248 }
Courier { Id = 381437312, Name = Yonatan Amir, Phone = 057-824-6247, Email = yonatan.amir@gmail.com, Password = yonatan#amir17, IsActive = True, TypeOfDelivery = Car, StartWorkTime = 27/08/2022 02:34:49, MaxDist = 85 }
Courier { Id = 900239429, Name = Michal Saban, Phone = 055-464-7900, Email = michal.saban@gmail.com, Password = michal#saban17, IsActive = False, TypeOfDelivery = Bicycle, StartWorkTime = 12/04/2024 03:12:26, MaxDist = 12 }
Courier { Id = 453641474, Name = Lior Gross, Phone = 051-876-7600, Email = lior.gross@gmail.com, Password = lior#gross04, IsActive = True, TypeOfDelivery = Bicycle, StartWorkTime = 23/11/2023 02:22:26, MaxDist = 11 }
Courier { Id = 337373253, Name = Roni Hasson, Phone = 057-139-3601, Email = roni.hasson@gmail.com, Password = roni#hasson02, IsActive = False, TypeOfDelivery = Car, StartWorkTime = 23/07/2025 05:54:22, MaxDist = 326 }
Courier { Id = 120917696, Name = Noam Ben-David, Phone = 051-684-2448, Email = noam.ben-david@gmail.com, Password = noam#ben-david09, IsActive = True, TypeOfDelivery = ByFoot, StartWorkTime = 05/04/2021 03:33:15, MaxDist = 5 }

-- Orders --
Order { Id = 1000, TypeOfOrder = Express, Address = Herzl 10, Tel Aviv, Latitude = 32.0675, Longitude = 34.7775, CustomerName = Noah Cohen, CustomerPhone = 053-747-2600, OrderOpeningTime = 27/09/2024 10:11:00, PackageDetails = Canvas painting - medium size, Description = Standard art delivery }
Order { Id = 1001, TypeOfOrder = Regular, Address = Jaffa Road 2, Jerusalem, Latitude = 31.778, Longitude = 35.2345, CustomerName = Daniel Levy, CustomerPhone = 055-790-8058, OrderOpeningTime = 04/04/2021 06:17:57, PackageDetails = Fragile - framed artwork, Description = Handle with care - original artwork }
Order { Id = 1002, TypeOfOrder = Regular, Address = Ha'arbaa 14, Herzliya, Latitude = 32.1632, Longitude = 34.8406, CustomerName = Maya Rosen, CustomerPhone = 050-688-3008, OrderOpeningTime = 12/05/2022 03:01:59, PackageDetails = Sculpture package - heavy, Description = Urgent exhibition piece - deliver directly to gallery }
Order { Id = 1003, TypeOfOrder = Regular, Address = Begin 101, Petah Tikva, Latitude = 32.088, Longitude = 34.8875, CustomerName = David Friedman, CustomerPhone = 050-892-2348, OrderOpeningTime = 13/05/2023 13:40:09, PackageDetails = Photography print envelope, Description = Check authenticity certificate before handover }
Order { Id = 1004, TypeOfOrder = Regular, Address = Sderot Hen 5, Haifa, Latitude = 32.804, Longitude = 34.9896, CustomerName = Lior Avrahami, CustomerPhone = 053-543-9086, OrderOpeningTime = 05/10/2022 05:35:14, PackageDetails = Limited edition art box, Description = Carefully packed for collector }
Order { Id = 1005, TypeOfOrder = Express, Address = Ha'atzmaut 25, Bat Yam, Latitude = 32.021, Longitude = 34.7544, CustomerName = Tamar Ben-David, CustomerPhone = 055-423-7504, OrderOpeningTime = 29/08/2024 13:28:10, PackageDetails = Canvas painting - medium size, Description = Standard art delivery }
Order { Id = 1006, TypeOfOrder = Express, Address = Modi'in, Latitude = 31.8989, Longitude = 35.0078, CustomerName = Eitan Barak, CustomerPhone = 056-214-5208, OrderOpeningTime = 22/10/2025 10:34:37, PackageDetails = Fragile - framed artwork, Description = Handle with care - original artwork }
Order { Id = 1007, TypeOfOrder = SameDay, Address = Ashdod, Latitude = 31.8044, Longitude = 34.6553, CustomerName = Shira Saban, CustomerPhone = 053-811-9913, OrderOpeningTime = 04/12/2023 01:53:53, PackageDetails = Sculpture package - heavy, Description = Urgent exhibition piece - deliver directly to gallery }
Order { Id = 1008, TypeOfOrder = SameDay, Address = Beersheba, Latitude = 31.2518, Longitude = 34.7915, CustomerName = Adam Danino, CustomerPhone = 055-417-4751, OrderOpeningTime = 18/01/2022 10:41:53, PackageDetails = Photography print envelope, Description = Check authenticity certificate before handover }
Order { Id = 1009, TypeOfOrder = Regular, Address = Rehovot, Latitude = 31.8948, Longitude = 34.811, CustomerName = Rachel Neuman, CustomerPhone = 053-374-2602, OrderOpeningTime = 20/05/2025 13:50:15, PackageDetails = Limited edition art box, Description = Carefully packed for collector }
Order { Id = 1010, TypeOfOrder = Regular, Address = Herzl 10, Tel Aviv, Latitude = 32.0675, Longitude = 34.7775, CustomerName = Itay Hershkovitz, CustomerPhone = 057-173-3738, OrderOpeningTime = 17/10/2022 09:21:31, PackageDetails = Canvas painting - medium size, Description = Standard art delivery }
Order { Id = 1011, TypeOfOrder = SameDay, Address = Jaffa Road 2, Jerusalem, Latitude = 31.778, Longitude = 35.2345, CustomerName = Roni Gabay, CustomerPhone = 053-521-2769, OrderOpeningTime = 08/08/2024 04:12:14, PackageDetails = Fragile - framed artwork, Description = Handle with care - original artwork }
Order { Id = 1012, TypeOfOrder = SameDay, Address = Ha'arbaa 14, Herzliya, Latitude = 32.1632, Longitude = 34.8406, CustomerName = Gal Zahavi, CustomerPhone = 053-804-1062, OrderOpeningTime = 07/04/2024 08:43:16, PackageDetails = Sculpture package - heavy, Description = Urgent exhibition piece - deliver directly to gallery }
Order { Id = 1013, TypeOfOrder = Express, Address = Begin 101, Petah Tikva, Latitude = 32.088, Longitude = 34.8875, CustomerName = Nina Ronen, CustomerPhone = 057-849-4854, OrderOpeningTime = 06/06/2025 14:06:25, PackageDetails = Photography print envelope, Description = Check authenticity certificate before handover }
Order { Id = 1014, TypeOfOrder = Regular, Address = Sderot Hen 5, Haifa, Latitude = 32.804, Longitude = 34.9896, CustomerName = Amir Ilan, CustomerPhone = 054-784-4284, OrderOpeningTime = 21/07/2023 05:47:49, PackageDetails = Limited edition art box, Description = Carefully packed for collector }
Order { Id = 1015, TypeOfOrder = Express, Address = Ha'atzmaut 25, Bat Yam, Latitude = 32.021, Longitude = 34.7544, CustomerName = Dana Marciano, CustomerPhone = 058-494-8678, OrderOpeningTime = 20/10/2021 11:54:57, PackageDetails = Canvas painting - medium size, Description = Standard art delivery }
Order { Id = 1016, TypeOfOrder = Express, Address = Modi'in, Latitude = 31.8989, Longitude = 35.0078, CustomerName = Jonathan Tzadok, CustomerPhone = 055-201-2891, OrderOpeningTime = 23/01/2023 09:53:02, PackageDetails = Fragile - framed artwork, Description = Handle with care - original artwork }
Order { Id = 1017, TypeOfOrder = Regular, Address = Ashdod, Latitude = 31.8044, Longitude = 34.6553, CustomerName = Liad Baruch, CustomerPhone = 052-418-1946, OrderOpeningTime = 10/11/2024 05:22:55, PackageDetails = Sculpture package - heavy, Description = Urgent exhibition piece - deliver directly to gallery }
Order { Id = 1018, TypeOfOrder = Regular, Address = Beersheba, Latitude = 31.2518, Longitude = 34.7915, CustomerName = Omer Amir, CustomerPhone = 050-363-5887, OrderOpeningTime = 24/07/2021 02:27:23, PackageDetails = Photography print envelope, Description = Check authenticity certificate before handover }
Order { Id = 1019, TypeOfOrder = Express, Address = Rehovot, Latitude = 31.8948, Longitude = 34.811, CustomerName = Ruth Hazan, CustomerPhone = 052-123-3389, OrderOpeningTime = 13/02/2022 07:24:10, PackageDetails = Limited edition art box, Description = Carefully packed for collector }
Order { Id = 1020, TypeOfOrder = Express, Address = Herzl 10, Tel Aviv, Latitude = 32.0675, Longitude = 34.7775, CustomerName = Eli Mor, CustomerPhone = 057-829-9286, OrderOpeningTime = 22/08/2022 05:05:15, PackageDetails = Canvas painting - medium size, Description = Standard art delivery }
Order { Id = 1021, TypeOfOrder = SameDay, Address = Jaffa Road 2, Jerusalem, Latitude = 31.778, Longitude = 35.2345, CustomerName = Sara Peretz, CustomerPhone = 055-156-2895, OrderOpeningTime = 16/11/2024 04:09:17, PackageDetails = Fragile - framed artwork, Description = Handle with care - original artwork }
Order { Id = 1022, TypeOfOrder = SameDay, Address = Ha'arbaa 14, Herzliya, Latitude = 32.1632, Longitude = 34.8406, CustomerName = Ben Avital, CustomerPhone = 058-465-3918, OrderOpeningTime = 12/04/2021 13:03:57, PackageDetails = Sculpture package - heavy, Description = Urgent exhibition piece - deliver directly to gallery }
Order { Id = 1023, TypeOfOrder = SameDay, Address = Begin 101, Petah Tikva, Latitude = 32.088, Longitude = 34.8875, CustomerName = Hila Levi, CustomerPhone = 058-842-3808, OrderOpeningTime = 31/01/2022 05:01:56, PackageDetails = Photography print envelope, Description = Check authenticity certificate before handover }
Order { Id = 1024, TypeOfOrder = Express, Address = Sderot Hen 5, Haifa, Latitude = 32.804, Longitude = 34.9896, CustomerName = Yoni Weiss, CustomerPhone = 050-782-5159, OrderOpeningTime = 27/12/2020 05:28:45, PackageDetails = Limited edition art box, Description = Carefully packed for collector }
Order { Id = 1025, TypeOfOrder = Regular, Address = Ha'atzmaut 25, Bat Yam, Latitude = 32.021, Longitude = 34.7544, CustomerName = Nadav Regev, CustomerPhone = 053-379-2783, OrderOpeningTime = 19/10/2021 04:42:58, PackageDetails = Canvas painting - medium size, Description = Standard art delivery }
Order { Id = 1026, TypeOfOrder = Express, Address = Modi'in, Latitude = 31.8989, Longitude = 35.0078, CustomerName = Tal Segal, CustomerPhone = 057-104-4101, OrderOpeningTime = 19/01/2022 05:34:00, PackageDetails = Fragile - framed artwork, Description = Handle with care - original artwork }
Order { Id = 1027, TypeOfOrder = SameDay, Address = Ashdod, Latitude = 31.8044, Longitude = 34.6553, CustomerName = Naomi Klein, CustomerPhone = 054-780-4668, OrderOpeningTime = 07/10/2023 06:14:06, PackageDetails = Sculpture package - heavy, Description = Urgent exhibition piece - deliver directly to gallery }
Order { Id = 1028, TypeOfOrder = Regular, Address = Beersheba, Latitude = 31.2518, Longitude = 34.7915, CustomerName = Oren Goldstein, CustomerPhone = 053-408-9028, OrderOpeningTime = 02/06/2021 03:00:16, PackageDetails = Photography print envelope, Description = Check authenticity certificate before handover }
Order { Id = 1029, TypeOfOrder = SameDay, Address = Rehovot, Latitude = 31.8948, Longitude = 34.811, CustomerName = Yael Mizrahi, CustomerPhone = 052-720-5836, OrderOpeningTime = 17/08/2021 14:22:39, PackageDetails = Limited edition art box, Description = Carefully packed for collector }
Order { Id = 1030, TypeOfOrder = Regular, Address = Herzl 10, Tel Aviv, Latitude = 32.0675, Longitude = 34.7775, CustomerName = Ariel Rubin, CustomerPhone = 050-400-5614, OrderOpeningTime = 16/09/2022 13:30:48, PackageDetails = Canvas painting - medium size, Description = Standard art delivery }
Order { Id = 1031, TypeOfOrder = Regular, Address = Jaffa Road 2, Jerusalem, Latitude = 31.778, Longitude = 35.2345, CustomerName = Noa Shahar, CustomerPhone = 057-538-5282, OrderOpeningTime = 02/12/2021 07:59:56, PackageDetails = Fragile - framed artwork, Description = Handle with care - original artwork }
Order { Id = 1032, TypeOfOrder = Express, Address = Ha'arbaa 14, Herzliya, Latitude = 32.1632, Longitude = 34.8406, CustomerName = Idan Azulay, CustomerPhone = 057-299-9496, OrderOpeningTime = 21/01/2024 07:17:01, PackageDetails = Sculpture package - heavy, Description = Urgent exhibition piece - deliver directly to gallery }
Order { Id = 1033, TypeOfOrder = Express, Address = Begin 101, Petah Tikva, Latitude = 32.088, Longitude = 34.8875, CustomerName = Romi Halevi, CustomerPhone = 055-982-5918, OrderOpeningTime = 31/05/2022 04:39:40, PackageDetails = Photography print envelope, Description = Check authenticity certificate before handover }
Order { Id = 1034, TypeOfOrder = Regular, Address = Sderot Hen 5, Haifa, Latitude = 32.804, Longitude = 34.9896, CustomerName = Erez Dayan, CustomerPhone = 054-292-4937, OrderOpeningTime = 04/03/2023 04:55:24, PackageDetails = Limited edition art box, Description = Carefully packed for collector }
Order { Id = 1035, TypeOfOrder = Express, Address = Ha'atzmaut 25, Bat Yam, Latitude = 32.021, Longitude = 34.7544, CustomerName = Galit Ashkenazi, CustomerPhone = 057-391-9263, OrderOpeningTime = 04/03/2025 02:15:24, PackageDetails = Canvas painting - medium size, Description = Standard art delivery }
Order { Id = 1036, TypeOfOrder = Regular, Address = Modi'in, Latitude = 31.8989, Longitude = 35.0078, CustomerName = Doron Koren, CustomerPhone = 052-897-9643, OrderOpeningTime = 30/09/2023 11:38:54, PackageDetails = Fragile - framed artwork, Description = Handle with care - original artwork }
Order { Id = 1037, TypeOfOrder = Regular, Address = Ashdod, Latitude = 31.8044, Longitude = 34.6553, CustomerName = Liat Morad, CustomerPhone = 056-306-1870, OrderOpeningTime = 12/04/2021 05:30:10, PackageDetails = Sculpture package - heavy, Description = Urgent exhibition piece - deliver directly to gallery }
Order { Id = 1038, TypeOfOrder = Regular, Address = Beersheba, Latitude = 31.2518, Longitude = 34.7915, CustomerName = Ori Katz, CustomerPhone = 051-816-9300, OrderOpeningTime = 02/01/2025 02:34:50, PackageDetails = Photography print envelope, Description = Check authenticity certificate before handover }
Order { Id = 1039, TypeOfOrder = Express, Address = Rehovot, Latitude = 31.8948, Longitude = 34.811, CustomerName = Ella Shemesh, CustomerPhone = 051-841-5671, OrderOpeningTime = 15/07/2023 01:33:45, PackageDetails = Limited edition art box, Description = Carefully packed for collector }
Order { Id = 1040, TypeOfOrder = SameDay, Address = Herzl 10, Tel Aviv, Latitude = 32.0675, Longitude = 34.7775, CustomerName = Ron Biton, CustomerPhone = 051-137-4808, OrderOpeningTime = 11/04/2025 04:05:04, PackageDetails = Canvas painting - medium size, Description = Standard art delivery }
Order { Id = 1041, TypeOfOrder = Regular, Address = Jaffa Road 2, Jerusalem, Latitude = 31.778, Longitude = 35.2345, CustomerName = Talia Oren, CustomerPhone = 054-307-4337, OrderOpeningTime = 02/01/2022 04:04:56, PackageDetails = Fragile - framed artwork, Description = Handle with care - original artwork }
Order { Id = 1042, TypeOfOrder = Regular, Address = Ha'arbaa 14, Herzliya, Latitude = 32.1632, Longitude = 34.8406, CustomerName = Gadi Ben-Haim, CustomerPhone = 053-774-1905, OrderOpeningTime = 28/05/2025 14:26:23, PackageDetails = Sculpture package - heavy, Description = Urgent exhibition piece - deliver directly to gallery }
Order { Id = 1043, TypeOfOrder = Express, Address = Begin 101, Petah Tikva, Latitude = 32.088, Longitude = 34.8875, CustomerName = Eden Cohen, CustomerPhone = 053-801-7471, OrderOpeningTime = 31/01/2021 01:48:06, PackageDetails = Photography print envelope, Description = Check authenticity certificate before handover }
Order { Id = 1044, TypeOfOrder = Express, Address = Sderot Hen 5, Haifa, Latitude = 32.804, Longitude = 34.9896, CustomerName = Noy Levi, CustomerPhone = 055-264-6858, OrderOpeningTime = 16/04/2022 13:49:39, PackageDetails = Limited edition art box, Description = Carefully packed for collector }
Order { Id = 1045, TypeOfOrder = Express, Address = Ha'atzmaut 25, Bat Yam, Latitude = 32.021, Longitude = 34.7544, CustomerName = Roy Shaked, CustomerPhone = 057-389-3893, OrderOpeningTime = 18/07/2025 05:56:55, PackageDetails = Canvas painting - medium size, Description = Standard art delivery }
Order { Id = 1046, TypeOfOrder = Express, Address = Modi'in, Latitude = 31.8989, Longitude = 35.0078, CustomerName = Alon Baruch, CustomerPhone = 050-524-7824, OrderOpeningTime = 25/02/2025 03:46:55, PackageDetails = Fragile - framed artwork, Description = Handle with care - original artwork }
Order { Id = 1047, TypeOfOrder = Express, Address = Ashdod, Latitude = 31.8044, Longitude = 34.6553, CustomerName = Michal Dahan, CustomerPhone = 051-880-9056, OrderOpeningTime = 24/12/2020 13:21:16, PackageDetails = Sculpture package - heavy, Description = Urgent exhibition piece - deliver directly to gallery }
Order { Id = 1048, TypeOfOrder = SameDay, Address = Beersheba, Latitude = 31.2518, Longitude = 34.7915, CustomerName = Yarden Golan, CustomerPhone = 058-220-2484, OrderOpeningTime = 11/04/2025 06:57:40, PackageDetails = Photography print envelope, Description = Check authenticity certificate before handover }
Order { Id = 1049, TypeOfOrder = Regular, Address = Rehovot, Latitude = 31.8948, Longitude = 34.811, CustomerName = Nadav Tal, CustomerPhone = 051-189-5258, OrderOpeningTime = 06/05/2022 13:52:07, PackageDetails = Limited edition art box, Description = Carefully packed for collector }

-- Deliveries --
Delivery { Id = 1000, OrderId = 1037, CourierId = 216276324, TypeOfOrder = Regular, DeliveryStartTime = 23/05/2024 04:50:10, ActualDistance = 33.437552023096465, OrderEndStatus = Cancelled, DeliveryEndTime = 23/05/2024 06:35:10 }
Delivery { Id = 1001, OrderId = 1047, CourierId = 216276324, TypeOfOrder = Express, DeliveryStartTime = 09/11/2022 18:13:16, ActualDistance = 33.437552023096465, OrderEndStatus = Refused, DeliveryEndTime = 09/11/2022 19:31:16 }
Delivery { Id = 1002, OrderId = 1007, CourierId = 961718182, TypeOfOrder = SameDay, DeliveryStartTime = 28/04/2025 17:05:53, ActualDistance = 33.437552023096465, OrderEndStatus = Refused, DeliveryEndTime = 28/04/2025 18:14:53 }
Delivery { Id = 1003, OrderId = 1019, CourierId = 964395664, TypeOfOrder = Express, DeliveryStartTime = 02/03/2023 17:56:10, ActualDistance = 21.36088640832011, OrderEndStatus = Delivered, DeliveryEndTime = 02/03/2023 19:58:10 }
Delivery { Id = 1004, OrderId = 1035, CourierId = 961718182, TypeOfOrder = Express, DeliveryStartTime = 04/10/2025 16:17:24, ActualDistance = 7.601862445755016, OrderEndStatus = Failed, DeliveryEndTime = 04/10/2025 17:24:24 }
Delivery { Id = 1005, OrderId = 1030, CourierId = 381437312, TypeOfOrder = Regular, DeliveryStartTime = 09/07/2023 14:13:48, ActualDistance = 2.0203098197726845, OrderEndStatus = Refused, DeliveryEndTime = 09/07/2023 17:08:48 }
Delivery { Id = 1006, OrderId = 1049, CourierId = 961718182, TypeOfOrder = Regular, DeliveryStartTime = 11/10/2022 06:22:07, ActualDistance = 21.36088640832011, OrderEndStatus = Refused, DeliveryEndTime = 11/10/2022 07:26:07 }
Delivery { Id = 1007, OrderId = 1018, CourierId = 825755264, TypeOfOrder = Regular, DeliveryStartTime = 11/10/2023 11:27:23, ActualDistance = 92.6855173262227, OrderEndStatus = Cancelled, DeliveryEndTime = 11/10/2023 12:37:23 }
Delivery { Id = 1008, OrderId = 1042, CourierId = 216276324, TypeOfOrder = Regular, DeliveryStartTime = 15/08/2025 19:49:23, ActualDistance = 10.28069354356626, OrderEndStatus = Failed, DeliveryEndTime = 15/08/2025 20:29:23 }
Delivery { Id = 1009, OrderId = 1045, CourierId = 900239429, TypeOfOrder = Express, DeliveryStartTime = 09/09/2025 23:17:55, ActualDistance = 7.601862445755016, OrderEndStatus = Delivered, DeliveryEndTime = 10/09/2025 01:21:55 }
Delivery { Id = 1010, OrderId = 1017, CourierId = 216276324, TypeOfOrder = Regular, DeliveryStartTime = 27/09/2025 12:54:55, ActualDistance = 33.437552023096465, OrderEndStatus = Delivered, DeliveryEndTime = 27/09/2025 15:28:55 }
Delivery { Id = 1011, OrderId = 1044, CourierId = 825755264, TypeOfOrder = Express, DeliveryStartTime = 13/01/2025 09:34:39, ActualDistance = 82.26031301295912, OrderEndStatus = Failed, DeliveryEndTime = 13/01/2025 10:19:39 }
Delivery { Id = 1012, OrderId = 1008, CourierId = 337373253, TypeOfOrder = SameDay, DeliveryStartTime = 30/11/2024 20:01:53, ActualDistance = 92.6855173262227, OrderEndStatus = InviterNotFound, DeliveryEndTime = 30/11/2024 22:55:53 }
Delivery { Id = 1013, OrderId = 1024, CourierId = 381437312, TypeOfOrder = Express, DeliveryStartTime = 10/03/2024 20:10:45, ActualDistance = 82.26031301295912, OrderEndStatus = InviterNotFound, DeliveryEndTime = 10/03/2024 20:53:45 }
Delivery { Id = 1014, OrderId = 1041, CourierId = 381437312, TypeOfOrder = Regular, DeliveryStartTime = 08/05/2025 02:28:56, ActualDistance = 54.705175646146685, OrderEndStatus = Cancelled, DeliveryEndTime = 08/05/2025 03:11:56 }
Delivery { Id = 1015, OrderId = 1029, CourierId = 381437312, TypeOfOrder = SameDay, DeliveryStartTime = 28/12/2024 20:26:39, ActualDistance = 21.36088640832011, OrderEndStatus = Cancelled, DeliveryEndTime = 28/12/2024 21:58:39 }
Delivery { Id = 1016, OrderId = 1023, CourierId = 961718182, TypeOfOrder = SameDay, DeliveryStartTime = 26/12/2024 07:01:56, ActualDistance = 9.9624606371655, OrderEndStatus = Delivered, DeliveryEndTime = 26/12/2024 07:23:56 }
Delivery { Id = 1017, OrderId = 1010, CourierId = 729177417, TypeOfOrder = Regular, DeliveryStartTime = 27/01/2023 10:08:31, ActualDistance = 2.0203098197726845, OrderEndStatus = Failed, DeliveryEndTime = 27/01/2023 11:39:31 }
Delivery { Id = 1018, OrderId = 1001, CourierId = 337373253, TypeOfOrder = Regular, DeliveryStartTime = 01/11/2023 04:33:57, ActualDistance = 54.705175646146685, OrderEndStatus = Delivered, DeliveryEndTime = 01/11/2023 07:24:57 }
Delivery { Id = 1019, OrderId = 1034, CourierId = 337373253, TypeOfOrder = Regular, DeliveryStartTime = 27/06/2023 21:02:24, ActualDistance = 82.26031301295912, OrderEndStatus = Cancelled, DeliveryEndTime = 27/06/2023 21:30:24 }
Delivery { Id = 1020, OrderId = 1031, CourierId = 825755264, TypeOfOrder = Regular, DeliveryStartTime = 31/10/2025 08:14:56, ActualDistance = 54.705175646146685, OrderEndStatus = , DeliveryEndTime =  }
Delivery { Id = 1021, OrderId = 1027, CourierId = 961718182, TypeOfOrder = SameDay, DeliveryStartTime = 26/10/2025 19:12:06, ActualDistance = 33.437552023096465, OrderEndStatus = , DeliveryEndTime =  }
Delivery { Id = 1022, OrderId = 1046, CourierId = 650604000, TypeOfOrder = Express, DeliveryStartTime = 22/06/2025 21:50:55, ActualDistance = 29.729690756933437, OrderEndStatus = , DeliveryEndTime =  }
Delivery { Id = 1023, OrderId = 1015, CourierId = 192256906, TypeOfOrder = Express, DeliveryStartTime = 08/03/2024 10:37:57, ActualDistance = 7.601862445755016, OrderEndStatus = , DeliveryEndTime =  }
Delivery { Id = 1024, OrderId = 1006, CourierId = 216276324, TypeOfOrder = Express, DeliveryStartTime = 01/11/2025 16:02:37, ActualDistance = 29.729690756933437, OrderEndStatus = , DeliveryEndTime =  }
Delivery { Id = 1025, OrderId = 1021, CourierId = 381437312, TypeOfOrder = SameDay, DeliveryStartTime = 03/10/2025 22:54:17, ActualDistance = 54.705175646146685, OrderEndStatus = , DeliveryEndTime =  }
Delivery { Id = 1026, OrderId = 1022, CourierId = 453641474, TypeOfOrder = SameDay, DeliveryStartTime = 22/04/2024 06:18:57, ActualDistance = 10.28069354356626, OrderEndStatus = , DeliveryEndTime =  }

--- END OF LIST ---

--- MAIN MENU ---
0: Exit
1: Courier Menu
2: Order Menu
3: Delivery Menu
4: Config Menu
5: Initialize Data (Reset and Create)
6: Reset All Data (Clear Lists & Config)
7: List All Data
Enter your choice: 1

--- COURIER MENU ---
0: Back to Main Menu
1: Create (Add new)
2: Read (Get by ID)
3: ReadAll (List all)
4: Update
5: Delete (By ID)
6: DeleteAll (Clear list)
Enter your choice: 1
Enter Courier ID (ID Card): 999999999
Enter Full Name: Test Courier
Enter Phone Number: 0501234567
Enter Email: test@gmail.com
Enter Password: test123
Is Active (true/false): true
Enter Delivery Type (Car, Motorcycle, Bicycle, ByFoot): Car
Enter Max Delivery Distance (leave empty for no limit): 50
Successfully added Courier 999999999 - Test Courier

--- COURIER MENU ---
0: Back to Main Menu
1: Create (Add new)
2: Read (Get by ID)
3: ReadAll (List all)
4: Update
5: Delete (By ID)
6: DeleteAll (Clear list)
Enter your choice: 2
Enter Courier ID to get: 999999999
Courier { Id = 999999999, Name = Test Courier, Phone = 0501234567, Email = test@gmail.com, Password = test123, IsActive = True, TypeOfDelivery = Car, StartWorkTime = 09/11/2025 18:33:22, MaxDist = 50 }

--- COURIER MENU ---
0: Back to Main Menu
1: Create (Add new)
2: Read (Get by ID)
3: ReadAll (List all)
4: Update
5: Delete (By ID)
6: DeleteAll (Clear list)
Enter your choice: 4
Enter Courier ID to update: 999999999
Current values:
Courier { Id = 999999999, Name = Test Courier, Phone = 0501234567, Email = test@gmail.com, Password = test123, IsActive = True, TypeOfDelivery = Car, StartWorkTime = 09/11/2025 18:33:22, MaxDist = 50 }
Enter new Name (current: Test Courier): Updated Name
Enter new Phone (current: 0501234567):
Enter new Email (current: test@gmail.com): new@email.com
Enter new Password (current: *****):
Enter new IsActive (current: True) (true/false):
Enter new Delivery Type (current: Car):
Enter new Max Delivery Distance (current: 50):
Successfully updated Courier 999999999

--- COURIER MENU ---
0: Back to Main Menu
1: Create (Add new)
2: Read (Get by ID)
3: ReadAll (List all)
4: Update
5: Delete (By ID)
6: DeleteAll (Clear list)
Enter your choice: 2
Enter Courier ID to get: 999999999
Courier { Id = 999999999, Name = Updated Name, Phone = 0501234567, Email = new@email.com, Password = test123, IsActive = True, TypeOfDelivery = Car, StartWorkTime = 09/11/2025 18:33:22, MaxDist = 50 }

--- COURIER MENU ---
0: Back to Main Menu
1: Create (Add new)
2: Read (Get by ID)
3: ReadAll (List all)
4: Update
5: Delete (By ID)
6: DeleteAll (Clear list)
Enter your choice: 5
Enter Courier ID to delete: 999999999
Successfully deleted Courier 999999999

--- COURIER MENU ---
0: Back to Main Menu
1: Create (Add new)
2: Read (Get by ID)
3: ReadAll (List all)
4: Update
5: Delete (By ID)
6: DeleteAll (Clear list)
Enter your choice: 2
Enter Courier ID to get: 999999999
Courier with ID=999999999 not found.

--- COURIER MENU ---
0: Back to Main Menu
1: Create (Add new)
2: Read (Get by ID)
3: ReadAll (List all)
4: Update
5: Delete (By ID)
6: DeleteAll (Clear list)
Enter your choice: 3
Courier { Id = 850665684, Name = Noa Levi, Phone = 058-793-6600, Email = noa.levi@gmail.com, Password = noa#levi19, IsActive = True, TypeOfDelivery = ByFoot, StartWorkTime = 05/05/2023 03:29:42, MaxDist = 4 }
Courier { Id = 460694135, Name = Daniel Cohen, Phone = 052-981-5362, Email = daniel.cohen@gmail.com, Password = daniel#cohen07, IsActive = True, TypeOfDelivery = ByFoot, StartWorkTime = 12/01/2025 02:25:34, MaxDist = 2 }
Courier { Id = 216276324, Name = Yael Barak, Phone = 053-895-4896, Email = yael.barak@gmail.com, Password = yael#barak00, IsActive = True, TypeOfDelivery = Motorcycle, StartWorkTime = 24/09/2021 05:55:00, MaxDist = 34 }
Courier { Id = 964395664, Name = Roi Avrahami, Phone = 056-192-4981, Email = roi.avrahami@gmail.com, Password = roi#avrahami15, IsActive = True, TypeOfDelivery = Motorcycle, StartWorkTime = 20/07/2021 02:58:06, MaxDist = 44 }
Courier { Id = 650604000, Name = Maya Friedman, Phone = 055-789-9199, Email = maya.friedman@gmail.com, Password = maya#friedman02, IsActive = True, TypeOfDelivery = Motorcycle, StartWorkTime = 12/08/2022 04:54:53, MaxDist = 35 }
Courier { Id = 961718182, Name = Omri Danino, Phone = 058-426-1492, Email = omri.danino@gmail.com, Password = omri#danino03, IsActive = True, TypeOfDelivery = Motorcycle, StartWorkTime = 09/11/2021 13:47:05, MaxDist = 36 }
Courier { Id = 537015669, Name = Tamar Rosen, Phone = 058-186-5302, Email = tamar.rosen@gmail.com, Password = tamar#rosen02, IsActive = False, TypeOfDelivery = Bicycle, StartWorkTime = 28/12/2020 11:30:45, MaxDist = 8 }
Courier { Id = 615022745, Name = Eitan Gabai, Phone = 057-116-4220, Email = eitan.gabai@gmail.com, Password = eitan#gabai12, IsActive = True, TypeOfDelivery = Motorcycle, StartWorkTime = 01/04/2024 07:59:10, MaxDist = 30 }
Courier { Id = 123711248, Name = Shira Neuman, Phone = 054-910-8630, Email = shira.neuman@gmail.com, Password = shira#neuman06, IsActive = True, TypeOfDelivery = ByFoot, StartWorkTime = 05/05/2025 13:21:50, MaxDist = 2 }
Courier { Id = 534280374, Name = Guy Hershkovitz, Phone = 055-181-3193, Email = guy.hershkovitz@gmail.com, Password = guy#hershkovitz07, IsActive = True, TypeOfDelivery = ByFoot, StartWorkTime = 16/12/2023 01:45:08, MaxDist = 1 }
Courier { Id = 729177417, Name = Alon Zahavi, Phone = 053-389-4064, Email = alon.zahavi@gmail.com, Password = alon#zahavi04, IsActive = True, TypeOfDelivery = ByFoot, StartWorkTime = 26/11/2021 04:18:05, MaxDist = 3 }
Courier { Id = 379579023, Name = Hila Ronen, Phone = 053-986-2956, Email = hila.ronen@gmail.com, Password = hila#ronen07, IsActive = True, TypeOfDelivery = Bicycle, StartWorkTime = 23/10/2022 09:07:38, MaxDist = 10 }
Courier { Id = 914548717, Name = Idan Marciano, Phone = 052-293-9108, Email = idan.marciano@gmail.com, Password = idan#marciano00, IsActive = True, TypeOfDelivery = Motorcycle, StartWorkTime = 21/06/2024 10:33:34, MaxDist = 15 }
Courier { Id = 192256906, Name = Rotem Tzadok, Phone = 054-135-1775, Email = rotem.tzadok@gmail.com, Password = rotem#tzadok20, IsActive = False, TypeOfDelivery = Bicycle, StartWorkTime = 31/05/2021 03:39:16, MaxDist = 8 }
Courier { Id = 825755264, Name = Adi Baruch, Phone = 053-173-6027, Email = adi.baruch@gmail.com, Password = adi#baruch20, IsActive = True, TypeOfDelivery = Car, StartWorkTime = 03/11/2022 11:50:08, MaxDist = 248 }
Courier { Id = 381437312, Name = Yonatan Amir, Phone = 057-824-6247, Email = yonatan.amir@gmail.com, Password = yonatan#amir17, IsActive = True, TypeOfDelivery = Car, StartWorkTime = 27/08/2022 02:34:49, MaxDist = 85 }
Courier { Id = 900239429, Name = Michal Saban, Phone = 055-464-7900, Email = michal.saban@gmail.com, Password = michal#saban17, IsActive = False, TypeOfDelivery = Bicycle, StartWorkTime = 12/04/2024 03:12:26, MaxDist = 12 }
Courier { Id = 453641474, Name = Lior Gross, Phone = 051-876-7600, Email = lior.gross@gmail.com, Password = lior#gross04, IsActive = True, TypeOfDelivery = Bicycle, StartWorkTime = 23/11/2023 02:22:26, MaxDist = 11 }
Courier { Id = 337373253, Name = Roni Hasson, Phone = 057-139-3601, Email = roni.hasson@gmail.com, Password = roni#hasson02, IsActive = False, TypeOfDelivery = Car, StartWorkTime = 23/07/2025 05:54:22, MaxDist = 326 }
Courier { Id = 120917696, Name = Noam Ben-David, Phone = 051-684-2448, Email = noam.ben-david@gmail.com, Password = noam#ben-david09, IsActive = True, TypeOfDelivery = ByFoot, StartWorkTime = 05/04/2021 03:33:15, MaxDist = 5 }

--- COURIER MENU ---
0: Back to Main Menu
1: Create (Add new)
2: Read (Get by ID)
3: ReadAll (List all)
4: Update
5: Delete (By ID)
6: DeleteAll (Clear list)
Enter your choice: 1
Enter Courier ID (ID Card): 337373253
Enter Full Name: Roni Hasson
Enter Phone Number: 0546537605
Enter Email: miri@gmail.com
Enter Password: 0515hren
Is Active (true/false): true
Enter Delivery Type (Car, Motorcycle, Bicycle, ByFoot): Car
Enter Max Delivery Distance (leave empty for no limit): 60
Error adding courier: Courier with Id 337373253 already exists.

--- COURIER MENU ---
0: Back to Main Menu
1: Create (Add new)
2: Read (Get by ID)
3: ReadAll (List all)
4: Update
5: Delete (By ID)
6: DeleteAll (Clear list)
Enter your choice: 0

--- MAIN MENU ---
0: Exit
1: Courier Menu
2: Order Menu
3: Delivery Menu
4: Config Menu
5: Initialize Data (Reset and Create)
6: Reset All Data (Clear Lists & Config)
7: List All Data
Enter your choice: 2

--- ORDER MENU ---
0: Back to Main Menu
1: Create (Add new)
2: Read (Get by ID)
3: ReadAll (List all)
4: Update
5: Delete (By ID)
6: DeleteAll (Clear list)
Enter your choice: 1
Enter Order Type (Regular, Express, SameDay): SameDay
Enter Address: givat zeev
Enter Latitude (Geographical coordinate): 32.41652
Enter Longitude (Geographical coordinate): 34.15658
Enter Customer Name: miri yaakobi
Enter Customer Phone: 0546537605
Enter Package Details (optional, press Enter to skip): prettey art
Enter Description (optional, press Enter to skip):
Successfully added new order for miri yaakobi

--- ORDER MENU ---
0: Back to Main Menu
1: Create (Add new)
2: Read (Get by ID)
3: ReadAll (List all)
4: Update
5: Delete (By ID)
6: DeleteAll (Clear list)
Enter your choice: 3
Order { Id = 1000, TypeOfOrder = Express, Address = Herzl 10, Tel Aviv, Latitude = 32.0675, Longitude = 34.7775, CustomerName = Noah Cohen, CustomerPhone = 053-747-2600, OrderOpeningTime = 27/09/2024 10:11:00, PackageDetails = Canvas painting - medium size, Description = Standard art delivery }
Order { Id = 1001, TypeOfOrder = Regular, Address = Jaffa Road 2, Jerusalem, Latitude = 31.778, Longitude = 35.2345, CustomerName = Daniel Levy, CustomerPhone = 055-790-8058, OrderOpeningTime = 04/04/2021 06:17:57, PackageDetails = Fragile - framed artwork, Description = Handle with care - original artwork }
Order { Id = 1002, TypeOfOrder = Regular, Address = Ha'arbaa 14, Herzliya, Latitude = 32.1632, Longitude = 34.8406, CustomerName = Maya Rosen, CustomerPhone = 050-688-3008, OrderOpeningTime = 12/05/2022 03:01:59, PackageDetails = Sculpture package - heavy, Description = Urgent exhibition piece - deliver directly to gallery }
Order { Id = 1003, TypeOfOrder = Regular, Address = Begin 101, Petah Tikva, Latitude = 32.088, Longitude = 34.8875, CustomerName = David Friedman, CustomerPhone = 050-892-2348, OrderOpeningTime = 13/05/2023 13:40:09, PackageDetails = Photography print envelope, Description = Check authenticity certificate before handover }
Order { Id = 1004, TypeOfOrder = Regular, Address = Sderot Hen 5, Haifa, Latitude = 32.804, Longitude = 34.9896, CustomerName = Lior Avrahami, CustomerPhone = 053-543-9086, OrderOpeningTime = 05/10/2022 05:35:14, PackageDetails = Limited edition art box, Description = Carefully packed for collector }
Order { Id = 1005, TypeOfOrder = Express, Address = Ha'atzmaut 25, Bat Yam, Latitude = 32.021, Longitude = 34.7544, CustomerName = Tamar Ben-David, CustomerPhone = 055-423-7504, OrderOpeningTime = 29/08/2024 13:28:10, PackageDetails = Canvas painting - medium size, Description = Standard art delivery }
Order { Id = 1006, TypeOfOrder = Express, Address = Modi'in, Latitude = 31.8989, Longitude = 35.0078, CustomerName = Eitan Barak, CustomerPhone = 056-214-5208, OrderOpeningTime = 22/10/2025 10:34:37, PackageDetails = Fragile - framed artwork, Description = Handle with care - original artwork }
Order { Id = 1007, TypeOfOrder = SameDay, Address = Ashdod, Latitude = 31.8044, Longitude = 34.6553, CustomerName = Shira Saban, CustomerPhone = 053-811-9913, OrderOpeningTime = 04/12/2023 01:53:53, PackageDetails = Sculpture package - heavy, Description = Urgent exhibition piece - deliver directly to gallery }
Order { Id = 1008, TypeOfOrder = SameDay, Address = Beersheba, Latitude = 31.2518, Longitude = 34.7915, CustomerName = Adam Danino, CustomerPhone = 055-417-4751, OrderOpeningTime = 18/01/2022 10:41:53, PackageDetails = Photography print envelope, Description = Check authenticity certificate before handover }
Order { Id = 1009, TypeOfOrder = Regular, Address = Rehovot, Latitude = 31.8948, Longitude = 34.811, CustomerName = Rachel Neuman, CustomerPhone = 053-374-2602, OrderOpeningTime = 20/05/2025 13:50:15, PackageDetails = Limited edition art box, Description = Carefully packed for collector }
Order { Id = 1010, TypeOfOrder = Regular, Address = Herzl 10, Tel Aviv, Latitude = 32.0675, Longitude = 34.7775, CustomerName = Itay Hershkovitz, CustomerPhone = 057-173-3738, OrderOpeningTime = 17/10/2022 09:21:31, PackageDetails = Canvas painting - medium size, Description = Standard art delivery }
Order { Id = 1011, TypeOfOrder = SameDay, Address = Jaffa Road 2, Jerusalem, Latitude = 31.778, Longitude = 35.2345, CustomerName = Roni Gabay, CustomerPhone = 053-521-2769, OrderOpeningTime = 08/08/2024 04:12:14, PackageDetails = Fragile - framed artwork, Description = Handle with care - original artwork }
Order { Id = 1012, TypeOfOrder = SameDay, Address = Ha'arbaa 14, Herzliya, Latitude = 32.1632, Longitude = 34.8406, CustomerName = Gal Zahavi, CustomerPhone = 053-804-1062, OrderOpeningTime = 07/04/2024 08:43:16, PackageDetails = Sculpture package - heavy, Description = Urgent exhibition piece - deliver directly to gallery }
Order { Id = 1013, TypeOfOrder = Express, Address = Begin 101, Petah Tikva, Latitude = 32.088, Longitude = 34.8875, CustomerName = Nina Ronen, CustomerPhone = 057-849-4854, OrderOpeningTime = 06/06/2025 14:06:25, PackageDetails = Photography print envelope, Description = Check authenticity certificate before handover }
Order { Id = 1014, TypeOfOrder = Regular, Address = Sderot Hen 5, Haifa, Latitude = 32.804, Longitude = 34.9896, CustomerName = Amir Ilan, CustomerPhone = 054-784-4284, OrderOpeningTime = 21/07/2023 05:47:49, PackageDetails = Limited edition art box, Description = Carefully packed for collector }
Order { Id = 1015, TypeOfOrder = Express, Address = Ha'atzmaut 25, Bat Yam, Latitude = 32.021, Longitude = 34.7544, CustomerName = Dana Marciano, CustomerPhone = 058-494-8678, OrderOpeningTime = 20/10/2021 11:54:57, PackageDetails = Canvas painting - medium size, Description = Standard art delivery }
Order { Id = 1016, TypeOfOrder = Express, Address = Modi'in, Latitude = 31.8989, Longitude = 35.0078, CustomerName = Jonathan Tzadok, CustomerPhone = 055-201-2891, OrderOpeningTime = 23/01/2023 09:53:02, PackageDetails = Fragile - framed artwork, Description = Handle with care - original artwork }
Order { Id = 1017, TypeOfOrder = Regular, Address = Ashdod, Latitude = 31.8044, Longitude = 34.6553, CustomerName = Liad Baruch, CustomerPhone = 052-418-1946, OrderOpeningTime = 10/11/2024 05:22:55, PackageDetails = Sculpture package - heavy, Description = Urgent exhibition piece - deliver directly to gallery }
Order { Id = 1018, TypeOfOrder = Regular, Address = Beersheba, Latitude = 31.2518, Longitude = 34.7915, CustomerName = Omer Amir, CustomerPhone = 050-363-5887, OrderOpeningTime = 24/07/2021 02:27:23, PackageDetails = Photography print envelope, Description = Check authenticity certificate before handover }
Order { Id = 1019, TypeOfOrder = Express, Address = Rehovot, Latitude = 31.8948, Longitude = 34.811, CustomerName = Ruth Hazan, CustomerPhone = 052-123-3389, OrderOpeningTime = 13/02/2022 07:24:10, PackageDetails = Limited edition art box, Description = Carefully packed for collector }
Order { Id = 1020, TypeOfOrder = Express, Address = Herzl 10, Tel Aviv, Latitude = 32.0675, Longitude = 34.7775, CustomerName = Eli Mor, CustomerPhone = 057-829-9286, OrderOpeningTime = 22/08/2022 05:05:15, PackageDetails = Canvas painting - medium size, Description = Standard art delivery }
Order { Id = 1021, TypeOfOrder = SameDay, Address = Jaffa Road 2, Jerusalem, Latitude = 31.778, Longitude = 35.2345, CustomerName = Sara Peretz, CustomerPhone = 055-156-2895, OrderOpeningTime = 16/11/2024 04:09:17, PackageDetails = Fragile - framed artwork, Description = Handle with care - original artwork }
Order { Id = 1022, TypeOfOrder = SameDay, Address = Ha'arbaa 14, Herzliya, Latitude = 32.1632, Longitude = 34.8406, CustomerName = Ben Avital, CustomerPhone = 058-465-3918, OrderOpeningTime = 12/04/2021 13:03:57, PackageDetails = Sculpture package - heavy, Description = Urgent exhibition piece - deliver directly to gallery }
Order { Id = 1023, TypeOfOrder = SameDay, Address = Begin 101, Petah Tikva, Latitude = 32.088, Longitude = 34.8875, CustomerName = Hila Levi, CustomerPhone = 058-842-3808, OrderOpeningTime = 31/01/2022 05:01:56, PackageDetails = Photography print envelope, Description = Check authenticity certificate before handover }
Order { Id = 1024, TypeOfOrder = Express, Address = Sderot Hen 5, Haifa, Latitude = 32.804, Longitude = 34.9896, CustomerName = Yoni Weiss, CustomerPhone = 050-782-5159, OrderOpeningTime = 27/12/2020 05:28:45, PackageDetails = Limited edition art box, Description = Carefully packed for collector }
Order { Id = 1025, TypeOfOrder = Regular, Address = Ha'atzmaut 25, Bat Yam, Latitude = 32.021, Longitude = 34.7544, CustomerName = Nadav Regev, CustomerPhone = 053-379-2783, OrderOpeningTime = 19/10/2021 04:42:58, PackageDetails = Canvas painting - medium size, Description = Standard art delivery }
Order { Id = 1026, TypeOfOrder = Express, Address = Modi'in, Latitude = 31.8989, Longitude = 35.0078, CustomerName = Tal Segal, CustomerPhone = 057-104-4101, OrderOpeningTime = 19/01/2022 05:34:00, PackageDetails = Fragile - framed artwork, Description = Handle with care - original artwork }
Order { Id = 1027, TypeOfOrder = SameDay, Address = Ashdod, Latitude = 31.8044, Longitude = 34.6553, CustomerName = Naomi Klein, CustomerPhone = 054-780-4668, OrderOpeningTime = 07/10/2023 06:14:06, PackageDetails = Sculpture package - heavy, Description = Urgent exhibition piece - deliver directly to gallery }
Order { Id = 1028, TypeOfOrder = Regular, Address = Beersheba, Latitude = 31.2518, Longitude = 34.7915, CustomerName = Oren Goldstein, CustomerPhone = 053-408-9028, OrderOpeningTime = 02/06/2021 03:00:16, PackageDetails = Photography print envelope, Description = Check authenticity certificate before handover }
Order { Id = 1029, TypeOfOrder = SameDay, Address = Rehovot, Latitude = 31.8948, Longitude = 34.811, CustomerName = Yael Mizrahi, CustomerPhone = 052-720-5836, OrderOpeningTime = 17/08/2021 14:22:39, PackageDetails = Limited edition art box, Description = Carefully packed for collector }
Order { Id = 1030, TypeOfOrder = Regular, Address = Herzl 10, Tel Aviv, Latitude = 32.0675, Longitude = 34.7775, CustomerName = Ariel Rubin, CustomerPhone = 050-400-5614, OrderOpeningTime = 16/09/2022 13:30:48, PackageDetails = Canvas painting - medium size, Description = Standard art delivery }
Order { Id = 1031, TypeOfOrder = Regular, Address = Jaffa Road 2, Jerusalem, Latitude = 31.778, Longitude = 35.2345, CustomerName = Noa Shahar, CustomerPhone = 057-538-5282, OrderOpeningTime = 02/12/2021 07:59:56, PackageDetails = Fragile - framed artwork, Description = Handle with care - original artwork }
Order { Id = 1032, TypeOfOrder = Express, Address = Ha'arbaa 14, Herzliya, Latitude = 32.1632, Longitude = 34.8406, CustomerName = Idan Azulay, CustomerPhone = 057-299-9496, OrderOpeningTime = 21/01/2024 07:17:01, PackageDetails = Sculpture package - heavy, Description = Urgent exhibition piece - deliver directly to gallery }
Order { Id = 1033, TypeOfOrder = Express, Address = Begin 101, Petah Tikva, Latitude = 32.088, Longitude = 34.8875, CustomerName = Romi Halevi, CustomerPhone = 055-982-5918, OrderOpeningTime = 31/05/2022 04:39:40, PackageDetails = Photography print envelope, Description = Check authenticity certificate before handover }
Order { Id = 1034, TypeOfOrder = Regular, Address = Sderot Hen 5, Haifa, Latitude = 32.804, Longitude = 34.9896, CustomerName = Erez Dayan, CustomerPhone = 054-292-4937, OrderOpeningTime = 04/03/2023 04:55:24, PackageDetails = Limited edition art box, Description = Carefully packed for collector }
Order { Id = 1035, TypeOfOrder = Express, Address = Ha'atzmaut 25, Bat Yam, Latitude = 32.021, Longitude = 34.7544, CustomerName = Galit Ashkenazi, CustomerPhone = 057-391-9263, OrderOpeningTime = 04/03/2025 02:15:24, PackageDetails = Canvas painting - medium size, Description = Standard art delivery }
Order { Id = 1036, TypeOfOrder = Regular, Address = Modi'in, Latitude = 31.8989, Longitude = 35.0078, CustomerName = Doron Koren, CustomerPhone = 052-897-9643, OrderOpeningTime = 30/09/2023 11:38:54, PackageDetails = Fragile - framed artwork, Description = Handle with care - original artwork }
Order { Id = 1037, TypeOfOrder = Regular, Address = Ashdod, Latitude = 31.8044, Longitude = 34.6553, CustomerName = Liat Morad, CustomerPhone = 056-306-1870, OrderOpeningTime = 12/04/2021 05:30:10, PackageDetails = Sculpture package - heavy, Description = Urgent exhibition piece - deliver directly to gallery }
Order { Id = 1038, TypeOfOrder = Regular, Address = Beersheba, Latitude = 31.2518, Longitude = 34.7915, CustomerName = Ori Katz, CustomerPhone = 051-816-9300, OrderOpeningTime = 02/01/2025 02:34:50, PackageDetails = Photography print envelope, Description = Check authenticity certificate before handover }
Order { Id = 1039, TypeOfOrder = Express, Address = Rehovot, Latitude = 31.8948, Longitude = 34.811, CustomerName = Ella Shemesh, CustomerPhone = 051-841-5671, OrderOpeningTime = 15/07/2023 01:33:45, PackageDetails = Limited edition art box, Description = Carefully packed for collector }
Order { Id = 1040, TypeOfOrder = SameDay, Address = Herzl 10, Tel Aviv, Latitude = 32.0675, Longitude = 34.7775, CustomerName = Ron Biton, CustomerPhone = 051-137-4808, OrderOpeningTime = 11/04/2025 04:05:04, PackageDetails = Canvas painting - medium size, Description = Standard art delivery }
Order { Id = 1041, TypeOfOrder = Regular, Address = Jaffa Road 2, Jerusalem, Latitude = 31.778, Longitude = 35.2345, CustomerName = Talia Oren, CustomerPhone = 054-307-4337, OrderOpeningTime = 02/01/2022 04:04:56, PackageDetails = Fragile - framed artwork, Description = Handle with care - original artwork }
Order { Id = 1042, TypeOfOrder = Regular, Address = Ha'arbaa 14, Herzliya, Latitude = 32.1632, Longitude = 34.8406, CustomerName = Gadi Ben-Haim, CustomerPhone = 053-774-1905, OrderOpeningTime = 28/05/2025 14:26:23, PackageDetails = Sculpture package - heavy, Description = Urgent exhibition piece - deliver directly to gallery }
Order { Id = 1043, TypeOfOrder = Express, Address = Begin 101, Petah Tikva, Latitude = 32.088, Longitude = 34.8875, CustomerName = Eden Cohen, CustomerPhone = 053-801-7471, OrderOpeningTime = 31/01/2021 01:48:06, PackageDetails = Photography print envelope, Description = Check authenticity certificate before handover }
Order { Id = 1044, TypeOfOrder = Express, Address = Sderot Hen 5, Haifa, Latitude = 32.804, Longitude = 34.9896, CustomerName = Noy Levi, CustomerPhone = 055-264-6858, OrderOpeningTime = 16/04/2022 13:49:39, PackageDetails = Limited edition art box, Description = Carefully packed for collector }
Order { Id = 1045, TypeOfOrder = Express, Address = Ha'atzmaut 25, Bat Yam, Latitude = 32.021, Longitude = 34.7544, CustomerName = Roy Shaked, CustomerPhone = 057-389-3893, OrderOpeningTime = 18/07/2025 05:56:55, PackageDetails = Canvas painting - medium size, Description = Standard art delivery }
Order { Id = 1046, TypeOfOrder = Express, Address = Modi'in, Latitude = 31.8989, Longitude = 35.0078, CustomerName = Alon Baruch, CustomerPhone = 050-524-7824, OrderOpeningTime = 25/02/2025 03:46:55, PackageDetails = Fragile - framed artwork, Description = Handle with care - original artwork }
Order { Id = 1047, TypeOfOrder = Express, Address = Ashdod, Latitude = 31.8044, Longitude = 34.6553, CustomerName = Michal Dahan, CustomerPhone = 051-880-9056, OrderOpeningTime = 24/12/2020 13:21:16, PackageDetails = Sculpture package - heavy, Description = Urgent exhibition piece - deliver directly to gallery }
Order { Id = 1048, TypeOfOrder = SameDay, Address = Beersheba, Latitude = 31.2518, Longitude = 34.7915, CustomerName = Yarden Golan, CustomerPhone = 058-220-2484, OrderOpeningTime = 11/04/2025 06:57:40, PackageDetails = Photography print envelope, Description = Check authenticity certificate before handover }
Order { Id = 1049, TypeOfOrder = Regular, Address = Rehovot, Latitude = 31.8948, Longitude = 34.811, CustomerName = Nadav Tal, CustomerPhone = 051-189-5258, OrderOpeningTime = 06/05/2022 13:52:07, PackageDetails = Limited edition art box, Description = Carefully packed for collector }
Order { Id = 1050, TypeOfOrder = SameDay, Address = givat zeev, Latitude = 32.41652, Longitude = 34.15658, CustomerName = miri yaakobi, CustomerPhone = 0546537605, OrderOpeningTime = 09/11/2025 18:33:22, PackageDetails = prettey art, Description =  }

--- ORDER MENU ---
0: Back to Main Menu
1: Create (Add new)
2: Read (Get by ID)
3: ReadAll (List all)
4: Update
5: Delete (By ID)
6: DeleteAll (Clear list)
Enter your choice: 4
Enter Order ID to update: 1050
Current values:
Order { Id = 1050, TypeOfOrder = SameDay, Address = givat zeev, Latitude = 32.41652, Longitude = 34.15658, CustomerName = miri yaakobi, CustomerPhone = 0546537605, OrderOpeningTime = 09/11/2025 18:33:22, PackageDetails = prettey art, Description =  }
Enter new Order Type (current: SameDay): SameDay
Enter new Address (current: givat zeev):
Enter new Latitude (current: 32.41652):
Enter new Longitude (current: 34.15658):
Enter new Customer Name (current: miri yaakobi):
Enter new Customer Phone (current: 0546537605):
Enter new Package Details (current: prettey art):
Enter new Description (current: ):
Successfully updated Order 1050

--- ORDER MENU ---
0: Back to Main Menu
1: Create (Add new)
2: Read (Get by ID)
3: ReadAll (List all)
4: Update
5: Delete (By ID)
6: DeleteAll (Clear list)
Enter your choice: 2
Enter Order ID to get: 1050
Order { Id = 1050, TypeOfOrder = SameDay, Address = givat zeev, Latitude = 32.41652, Longitude = 34.15658, CustomerName = miri yaakobi, CustomerPhone = 0546537605, OrderOpeningTime = 09/11/2025 18:33:22, PackageDetails = , Description =  }

--- ORDER MENU ---
0: Back to Main Menu
1: Create (Add new)
2: Read (Get by ID)
3: ReadAll (List all)
4: Update
5: Delete (By ID)
6: DeleteAll (Clear list)
Enter your choice: 5
Enter Order ID to delete: 1050
Successfully deleted Order 1050

--- ORDER MENU ---
0: Back to Main Menu
1: Create (Add new)
2: Read (Get by ID)
3: ReadAll (List all)
4: Update
5: Delete (By ID)
6: DeleteAll (Clear list)
Enter your choice: 0

--- MAIN MENU ---
0: Exit
1: Courier Menu
2: Order Menu
3: Delivery Menu
4: Config Menu
5: Initialize Data (Reset and Create)
6: Reset All Data (Clear Lists & Config)
7: List All Data
Enter your choice: 3

--- DELIVERY MENU ---
0: Back to Main Menu
1: Create (Add new)
2: Read (Get by ID)
3: ReadAll (List all)
4: Update
5: Delete (By ID)
6: DeleteAll (Clear list)
Enter your choice: 1
Enter Order ID to assign: 8065
Enter Courier ID to assign: 329232540
Enter Order Type (Regular, Express, SameDay): Regular
Successfully created new delivery, assigning Order 8065 to Courier 329232540

--- DELIVERY MENU ---
0: Back to Main Menu
1: Create (Add new)
2: Read (Get by ID)
3: ReadAll (List all)
4: Update
5: Delete (By ID)
6: DeleteAll (Clear list)
Enter your choice: 3
Delivery { Id = 1000, OrderId = 1037, CourierId = 216276324, TypeOfOrder = Regular, DeliveryStartTime = 23/05/2024 04:50:10, ActualDistance = 33.437552023096465, OrderEndStatus = Cancelled, DeliveryEndTime = 23/05/2024 06:35:10 }
Delivery { Id = 1001, OrderId = 1047, CourierId = 216276324, TypeOfOrder = Express, DeliveryStartTime = 09/11/2022 18:13:16, ActualDistance = 33.437552023096465, OrderEndStatus = Refused, DeliveryEndTime = 09/11/2022 19:31:16 }
Delivery { Id = 1002, OrderId = 1007, CourierId = 961718182, TypeOfOrder = SameDay, DeliveryStartTime = 28/04/2025 17:05:53, ActualDistance = 33.437552023096465, OrderEndStatus = Refused, DeliveryEndTime = 28/04/2025 18:14:53 }
Delivery { Id = 1003, OrderId = 1019, CourierId = 964395664, TypeOfOrder = Express, DeliveryStartTime = 02/03/2023 17:56:10, ActualDistance = 21.36088640832011, OrderEndStatus = Delivered, DeliveryEndTime = 02/03/2023 19:58:10 }
Delivery { Id = 1004, OrderId = 1035, CourierId = 961718182, TypeOfOrder = Express, DeliveryStartTime = 04/10/2025 16:17:24, ActualDistance = 7.601862445755016, OrderEndStatus = Failed, DeliveryEndTime = 04/10/2025 17:24:24 }
Delivery { Id = 1005, OrderId = 1030, CourierId = 381437312, TypeOfOrder = Regular, DeliveryStartTime = 09/07/2023 14:13:48, ActualDistance = 2.0203098197726845, OrderEndStatus = Refused, DeliveryEndTime = 09/07/2023 17:08:48 }
Delivery { Id = 1006, OrderId = 1049, CourierId = 961718182, TypeOfOrder = Regular, DeliveryStartTime = 11/10/2022 06:22:07, ActualDistance = 21.36088640832011, OrderEndStatus = Refused, DeliveryEndTime = 11/10/2022 07:26:07 }
Delivery { Id = 1007, OrderId = 1018, CourierId = 825755264, TypeOfOrder = Regular, DeliveryStartTime = 11/10/2023 11:27:23, ActualDistance = 92.6855173262227, OrderEndStatus = Cancelled, DeliveryEndTime = 11/10/2023 12:37:23 }
Delivery { Id = 1008, OrderId = 1042, CourierId = 216276324, TypeOfOrder = Regular, DeliveryStartTime = 15/08/2025 19:49:23, ActualDistance = 10.28069354356626, OrderEndStatus = Failed, DeliveryEndTime = 15/08/2025 20:29:23 }
Delivery { Id = 1009, OrderId = 1045, CourierId = 900239429, TypeOfOrder = Express, DeliveryStartTime = 09/09/2025 23:17:55, ActualDistance = 7.601862445755016, OrderEndStatus = Delivered, DeliveryEndTime = 10/09/2025 01:21:55 }
Delivery { Id = 1010, OrderId = 1017, CourierId = 216276324, TypeOfOrder = Regular, DeliveryStartTime = 27/09/2025 12:54:55, ActualDistance = 33.437552023096465, OrderEndStatus = Delivered, DeliveryEndTime = 27/09/2025 15:28:55 }
Delivery { Id = 1011, OrderId = 1044, CourierId = 825755264, TypeOfOrder = Express, DeliveryStartTime = 13/01/2025 09:34:39, ActualDistance = 82.26031301295912, OrderEndStatus = Failed, DeliveryEndTime = 13/01/2025 10:19:39 }
Delivery { Id = 1012, OrderId = 1008, CourierId = 337373253, TypeOfOrder = SameDay, DeliveryStartTime = 30/11/2024 20:01:53, ActualDistance = 92.6855173262227, OrderEndStatus = InviterNotFound, DeliveryEndTime = 30/11/2024 22:55:53 }
Delivery { Id = 1013, OrderId = 1024, CourierId = 381437312, TypeOfOrder = Express, DeliveryStartTime = 10/03/2024 20:10:45, ActualDistance = 82.26031301295912, OrderEndStatus = InviterNotFound, DeliveryEndTime = 10/03/2024 20:53:45 }
Delivery { Id = 1014, OrderId = 1041, CourierId = 381437312, TypeOfOrder = Regular, DeliveryStartTime = 08/05/2025 02:28:56, ActualDistance = 54.705175646146685, OrderEndStatus = Cancelled, DeliveryEndTime = 08/05/2025 03:11:56 }
Delivery { Id = 1015, OrderId = 1029, CourierId = 381437312, TypeOfOrder = SameDay, DeliveryStartTime = 28/12/2024 20:26:39, ActualDistance = 21.36088640832011, OrderEndStatus = Cancelled, DeliveryEndTime = 28/12/2024 21:58:39 }
Delivery { Id = 1016, OrderId = 1023, CourierId = 961718182, TypeOfOrder = SameDay, DeliveryStartTime = 26/12/2024 07:01:56, ActualDistance = 9.9624606371655, OrderEndStatus = Delivered, DeliveryEndTime = 26/12/2024 07:23:56 }
Delivery { Id = 1017, OrderId = 1010, CourierId = 729177417, TypeOfOrder = Regular, DeliveryStartTime = 27/01/2023 10:08:31, ActualDistance = 2.0203098197726845, OrderEndStatus = Failed, DeliveryEndTime = 27/01/2023 11:39:31 }
Delivery { Id = 1018, OrderId = 1001, CourierId = 337373253, TypeOfOrder = Regular, DeliveryStartTime = 01/11/2023 04:33:57, ActualDistance = 54.705175646146685, OrderEndStatus = Delivered, DeliveryEndTime = 01/11/2023 07:24:57 }
Delivery { Id = 1019, OrderId = 1034, CourierId = 337373253, TypeOfOrder = Regular, DeliveryStartTime = 27/06/2023 21:02:24, ActualDistance = 82.26031301295912, OrderEndStatus = Cancelled, DeliveryEndTime = 27/06/2023 21:30:24 }
Delivery { Id = 1020, OrderId = 1031, CourierId = 825755264, TypeOfOrder = Regular, DeliveryStartTime = 31/10/2025 08:14:56, ActualDistance = 54.705175646146685, OrderEndStatus = , DeliveryEndTime =  }
Delivery { Id = 1021, OrderId = 1027, CourierId = 961718182, TypeOfOrder = SameDay, DeliveryStartTime = 26/10/2025 19:12:06, ActualDistance = 33.437552023096465, OrderEndStatus = , DeliveryEndTime =  }
Delivery { Id = 1022, OrderId = 1046, CourierId = 650604000, TypeOfOrder = Express, DeliveryStartTime = 22/06/2025 21:50:55, ActualDistance = 29.729690756933437, OrderEndStatus = , DeliveryEndTime =  }
Delivery { Id = 1023, OrderId = 1015, CourierId = 192256906, TypeOfOrder = Express, DeliveryStartTime = 08/03/2024 10:37:57, ActualDistance = 7.601862445755016, OrderEndStatus = , DeliveryEndTime =  }
Delivery { Id = 1024, OrderId = 1006, CourierId = 216276324, TypeOfOrder = Express, DeliveryStartTime = 01/11/2025 16:02:37, ActualDistance = 29.729690756933437, OrderEndStatus = , DeliveryEndTime =  }
Delivery { Id = 1025, OrderId = 1021, CourierId = 381437312, TypeOfOrder = SameDay, DeliveryStartTime = 03/10/2025 22:54:17, ActualDistance = 54.705175646146685, OrderEndStatus = , DeliveryEndTime =  }
Delivery { Id = 1026, OrderId = 1022, CourierId = 453641474, TypeOfOrder = SameDay, DeliveryStartTime = 22/04/2024 06:18:57, ActualDistance = 10.28069354356626, OrderEndStatus = , DeliveryEndTime =  }
Delivery { Id = 1027, OrderId = 8065, CourierId = 329232540, TypeOfOrder = Regular, DeliveryStartTime = 09/11/2025 18:33:22, ActualDistance = , OrderEndStatus = , DeliveryEndTime =  }

--- DELIVERY MENU ---
0: Back to Main Menu
1: Create (Add new)
2: Read (Get by ID)
3: ReadAll (List all)
4: Update
5: Delete (By ID)
6: DeleteAll (Clear list)
Enter your choice: 4
Enter Delivery ID to update: 1027
Current values:
Delivery { Id = 1027, OrderId = 8065, CourierId = 329232540, TypeOfOrder = Regular, DeliveryStartTime = 09/11/2025 18:33:22, ActualDistance = , OrderEndStatus = , DeliveryEndTime =  }
Enter new Actual Distance (current: ): 356
Enter new Order End Status (current: ): 456
Enter new Delivery End Time (current: ): 10:55
Successfully updated Delivery 1027

--- DELIVERY MENU ---
0: Back to Main Menu
1: Create (Add new)
2: Read (Get by ID)
3: ReadAll (List all)
4: Update
5: Delete (By ID)
6: DeleteAll (Clear list)
Enter your choice: 2
Enter Delivery ID to get: 1027
Delivery { Id = 1027, OrderId = 8065, CourierId = 329232540, TypeOfOrder = Regular, DeliveryStartTime = 09/11/2025 18:33:22, ActualDistance = 356, OrderEndStatus = 456, DeliveryEndTime = 09/11/2025 10:55:00 }

--- DELIVERY MENU ---
0: Back to Main Menu
1: Create (Add new)
2: Read (Get by ID)
3: ReadAll (List all)
4: Update
5: Delete (By ID)
6: DeleteAll (Clear list)
Enter your choice: 5
Enter Delivery ID to delete: 1027
Successfully deleted Delivery 1027

--- DELIVERY MENU ---
0: Back to Main Menu
1: Create (Add new)
2: Read (Get by ID)
3: ReadAll (List all)
4: Update
5: Delete (By ID)
6: DeleteAll (Clear list)
Enter your choice: 0

--- MAIN MENU ---
0: Exit
1: Courier Menu
2: Order Menu
3: Delivery Menu
4: Config Menu
5: Initialize Data (Reset and Create)
6: Reset All Data (Clear Lists & Config)
7: List All Data
Enter your choice: 4

--- CONFIGURATION MENU ---
0: Back to Main Menu
1: Add Minute
2: Add Hour
3: Add Day
4: Add Week
5: Add Month
6: Add Year
7: Show Current Clock
8: Update Config Variable
9: Show Config Variable
10: Reset All Config
Enter your choice: 7
Current System Clock: 09/11/2025 18:33:22

--- CONFIGURATION MENU ---
0: Back to Main Menu
1: Add Minute
2: Add Hour
3: Add Day
4: Add Week
5: Add Month
6: Add Year
7: Show Current Clock
8: Update Config Variable
9: Show Config Variable
10: Reset All Config
Enter your choice: 2
System clock advanced from 09/11/2025 18:33:22 to 09/11/2025 19:33:22

--- CONFIGURATION MENU ---
0: Back to Main Menu
1: Add Minute
2: Add Hour
3: Add Day
4: Add Week
5: Add Month
6: Add Year
7: Show Current Clock
8: Update Config Variable
9: Show Config Variable
10: Reset All Config
Enter your choice: 7
Current System Clock: 09/11/2025 19:33:22

--- CONFIGURATION MENU ---
0: Back to Main Menu
1: Add Minute
2: Add Hour
3: Add Day
4: Add Week
5: Add Month
6: Add Year
7: Show Current Clock
8: Update Config Variable
9: Show Config Variable
10: Reset All Config
Enter your choice: 0

--- MAIN MENU ---
0: Exit
1: Courier Menu
2: Order Menu
3: Delivery Menu
4: Config Menu
5: Initialize Data (Reset and Create)
6: Reset All Data (Clear Lists & Config)
7: List All Data
Enter your choice: 6
Resetting all data...
All data reset.

--- MAIN MENU ---
0: Exit
1: Courier Menu
2: Order Menu
3: Delivery Menu
4: Config Menu
5: Initialize Data (Reset and Create)
6: Reset All Data (Clear Lists & Config)
7: List All Data
Enter your choice: 7

--- LISTING ALL DATA ---

-- Couriers --
No couriers found in the database.

-- Orders --
No orders found in the database.

-- Deliveries --
No deliveries found in the database.

--- END OF LIST ---

--- MAIN MENU ---
0: Exit
1: Courier Menu
2: Order Menu
3: Delivery Menu
4: Config Menu
5: Initialize Data (Reset and Create)
6: Reset All Data (Clear Lists & Config)
7: List All Data
Enter your choice: 5
Re-initializing data (Reset + Create)...
Reset Configuration values and List values...
Initializing Couriers...
Initializing Orders...
Initializing Deliveries...
Initialization done.
Data re-initialized successfully.

--- MAIN MENU ---
0: Exit
1: Courier Menu
2: Order Menu
3: Delivery Menu
4: Config Menu
5: Initialize Data (Reset and Create)
6: Reset All Data (Clear Lists & Config)
7: List All Data
Enter your choice: 7

--- LISTING ALL DATA ---

-- Couriers --
Courier { Id = 344926647, Name = Noa Levi, Phone = 052-811-9273, Email = noa.levi@gmail.com, Password = noa#levi11, IsActive = False, TypeOfDelivery = Motorcycle, StartWorkTime = 06/05/2024 03:35:37, MaxDist = 37 }
Courier { Id = 323019443, Name = Daniel Cohen, Phone = 056-655-8089, Email = daniel.cohen@gmail.com, Password = daniel#cohen10, IsActive = True, TypeOfDelivery = Bicycle, StartWorkTime = 17/10/2021 11:02:05, MaxDist = 14 }
Courier { Id = 212208485, Name = Yael Barak, Phone = 053-180-4488, Email = yael.barak@gmail.com, Password = yael#barak02, IsActive = True, TypeOfDelivery = ByFoot, StartWorkTime = 19/02/2023 13:33:30, MaxDist = 1 }
Courier { Id = 468490549, Name = Roi Avrahami, Phone = 055-470-9818, Email = roi.avrahami@gmail.com, Password = roi#avrahami09, IsActive = True, TypeOfDelivery = Bicycle, StartWorkTime = 22/10/2022 15:37:44, MaxDist = 2 }
Courier { Id = 853928876, Name = Maya Friedman, Phone = 058-577-3510, Email = maya.friedman@gmail.com, Password = maya#friedman18, IsActive = True, TypeOfDelivery = Motorcycle, StartWorkTime = 02/02/2022 04:24:41, MaxDist = 16 }
Courier { Id = 545826872, Name = Omri Danino, Phone = 054-635-2603, Email = omri.danino@gmail.com, Password = omri#danino09, IsActive = True, TypeOfDelivery = Motorcycle, StartWorkTime = 23/09/2021 05:58:28, MaxDist = 45 }
Courier { Id = 141244895, Name = Tamar Rosen, Phone = 055-474-1437, Email = tamar.rosen@gmail.com, Password = tamar#rosen06, IsActive = True, TypeOfDelivery = Motorcycle, StartWorkTime = 06/05/2022 06:41:02, MaxDist = 30 }
Courier { Id = 262581120, Name = Eitan Gabai, Phone = 051-945-9086, Email = eitan.gabai@gmail.com, Password = eitan#gabai17, IsActive = True, TypeOfDelivery = Car, StartWorkTime = 07/08/2022 05:48:16, MaxDist = 230 }
Courier { Id = 455753034, Name = Shira Neuman, Phone = 057-319-2645, Email = shira.neuman@gmail.com, Password = shira#neuman14, IsActive = True, TypeOfDelivery = ByFoot, StartWorkTime = 15/08/2021 04:32:10, MaxDist = 4 }
Courier { Id = 642880051, Name = Guy Hershkovitz, Phone = 050-941-7964, Email = guy.hershkovitz@gmail.com, Password = guy#hershkovitz12, IsActive = True, TypeOfDelivery = ByFoot, StartWorkTime = 31/08/2021 04:13:51, MaxDist = 3 }
Courier { Id = 483649597, Name = Alon Zahavi, Phone = 055-532-1777, Email = alon.zahavi@gmail.com, Password = alon#zahavi01, IsActive = True, TypeOfDelivery = Car, StartWorkTime = 18/12/2024 06:29:04, MaxDist = 144 }
Courier { Id = 443443968, Name = Hila Ronen, Phone = 058-151-4267, Email = hila.ronen@gmail.com, Password = hila#ronen07, IsActive = False, TypeOfDelivery = Bicycle, StartWorkTime = 18/06/2022 04:30:28, MaxDist = 2 }
Courier { Id = 543020197, Name = Idan Marciano, Phone = 051-130-4594, Email = idan.marciano@gmail.com, Password = idan#marciano02, IsActive = True, TypeOfDelivery = Motorcycle, StartWorkTime = 10/10/2025 07:13:04, MaxDist = 31 }
Courier { Id = 340696032, Name = Rotem Tzadok, Phone = 056-544-7236, Email = rotem.tzadok@gmail.com, Password = rotem#tzadok05, IsActive = False, TypeOfDelivery = ByFoot, StartWorkTime = 05/10/2024 10:26:26, MaxDist = 3 }
Courier { Id = 956087862, Name = Adi Baruch, Phone = 051-302-4178, Email = adi.baruch@gmail.com, Password = adi#baruch08, IsActive = True, TypeOfDelivery = Car, StartWorkTime = 05/12/2021 04:57:44, MaxDist = 265 }
Courier { Id = 391874673, Name = Yonatan Amir, Phone = 055-772-2489, Email = yonatan.amir@gmail.com, Password = yonatan#amir09, IsActive = True, TypeOfDelivery = Motorcycle, StartWorkTime = 31/10/2025 11:01:24, MaxDist = 8 }
Courier { Id = 452657218, Name = Michal Saban, Phone = 054-533-8688, Email = michal.saban@gmail.com, Password = michal#saban02, IsActive = True, TypeOfDelivery = Bicycle, StartWorkTime = 22/07/2022 06:29:10, MaxDist = 4 }
Courier { Id = 852499651, Name = Lior Gross, Phone = 056-848-6668, Email = lior.gross@gmail.com, Password = lior#gross07, IsActive = False, TypeOfDelivery = ByFoot, StartWorkTime = 17/01/2022 07:49:21, MaxDist = 2 }
Courier { Id = 694180588, Name = Roni Hasson, Phone = 051-795-7167, Email = roni.hasson@gmail.com, Password = roni#hasson06, IsActive = True, TypeOfDelivery = Motorcycle, StartWorkTime = 19/02/2021 08:09:06, MaxDist = 46 }
Courier { Id = 590118231, Name = Noam Ben-David, Phone = 050-259-9548, Email = noam.ben-david@gmail.com, Password = noam#ben-david00, IsActive = False, TypeOfDelivery = ByFoot, StartWorkTime = 29/05/2021 14:15:40, MaxDist = 2 }

-- Orders --
Order { Id = 1000, TypeOfOrder = Express, Address = Herzl 10, Tel Aviv, Latitude = 32.0675, Longitude = 34.7775, CustomerName = Noah Cohen, CustomerPhone = 052-570-1389, OrderOpeningTime = 16/12/2022 13:39:28, PackageDetails = Canvas painting - medium size, Description = Standard art delivery }
Order { Id = 1001, TypeOfOrder = SameDay, Address = Jaffa Road 2, Jerusalem, Latitude = 31.778, Longitude = 35.2345, CustomerName = Daniel Levy, CustomerPhone = 052-950-2076, OrderOpeningTime = 12/11/2024 13:31:20, PackageDetails = Fragile - framed artwork, Description = Handle with care - original artwork }
Order { Id = 1002, TypeOfOrder = SameDay, Address = Ha'arbaa 14, Herzliya, Latitude = 32.1632, Longitude = 34.8406, CustomerName = Maya Rosen, CustomerPhone = 057-378-6004, OrderOpeningTime = 03/11/2025 06:36:38, PackageDetails = Sculpture package - heavy, Description = Urgent exhibition piece - deliver directly to gallery }
Order { Id = 1003, TypeOfOrder = SameDay, Address = Begin 101, Petah Tikva, Latitude = 32.088, Longitude = 34.8875, CustomerName = David Friedman, CustomerPhone = 053-102-5945, OrderOpeningTime = 19/01/2021 02:54:21, PackageDetails = Photography print envelope, Description = Check authenticity certificate before handover }
Order { Id = 1004, TypeOfOrder = Regular, Address = Sderot Hen 5, Haifa, Latitude = 32.804, Longitude = 34.9896, CustomerName = Lior Avrahami, CustomerPhone = 058-269-3669, OrderOpeningTime = 25/02/2023 03:08:33, PackageDetails = Limited edition art box, Description = Carefully packed for collector }
Order { Id = 1005, TypeOfOrder = Regular, Address = Ha'atzmaut 25, Bat Yam, Latitude = 32.021, Longitude = 34.7544, CustomerName = Tamar Ben-David, CustomerPhone = 058-448-9218, OrderOpeningTime = 24/07/2022 05:42:25, PackageDetails = Canvas painting - medium size, Description = Standard art delivery }
Order { Id = 1006, TypeOfOrder = Express, Address = Modi'in, Latitude = 31.8989, Longitude = 35.0078, CustomerName = Eitan Barak, CustomerPhone = 052-955-3245, OrderOpeningTime = 25/08/2022 02:31:23, PackageDetails = Fragile - framed artwork, Description = Handle with care - original artwork }
Order { Id = 1007, TypeOfOrder = SameDay, Address = Ashdod, Latitude = 31.8044, Longitude = 34.6553, CustomerName = Shira Saban, CustomerPhone = 058-487-8677, OrderOpeningTime = 15/11/2020 09:21:42, PackageDetails = Sculpture package - heavy, Description = Urgent exhibition piece - deliver directly to gallery }
Order { Id = 1008, TypeOfOrder = Regular, Address = Beersheba, Latitude = 31.2518, Longitude = 34.7915, CustomerName = Adam Danino, CustomerPhone = 054-459-3608, OrderOpeningTime = 02/03/2024 03:07:36, PackageDetails = Photography print envelope, Description = Check authenticity certificate before handover }
Order { Id = 1009, TypeOfOrder = SameDay, Address = Rehovot, Latitude = 31.8948, Longitude = 34.811, CustomerName = Rachel Neuman, CustomerPhone = 051-819-7221, OrderOpeningTime = 15/01/2024 05:27:45, PackageDetails = Limited edition art box, Description = Carefully packed for collector }
Order { Id = 1010, TypeOfOrder = SameDay, Address = Herzl 10, Tel Aviv, Latitude = 32.0675, Longitude = 34.7775, CustomerName = Itay Hershkovitz, CustomerPhone = 054-330-1770, OrderOpeningTime = 28/08/2024 05:13:27, PackageDetails = Canvas painting - medium size, Description = Standard art delivery }
Order { Id = 1011, TypeOfOrder = SameDay, Address = Jaffa Road 2, Jerusalem, Latitude = 31.778, Longitude = 35.2345, CustomerName = Roni Gabay, CustomerPhone = 052-856-3801, OrderOpeningTime = 23/12/2024 03:36:31, PackageDetails = Fragile - framed artwork, Description = Handle with care - original artwork }
Order { Id = 1012, TypeOfOrder = Express, Address = Ha'arbaa 14, Herzliya, Latitude = 32.1632, Longitude = 34.8406, CustomerName = Gal Zahavi, CustomerPhone = 054-849-3102, OrderOpeningTime = 21/04/2024 10:37:46, PackageDetails = Sculpture package - heavy, Description = Urgent exhibition piece - deliver directly to gallery }
Order { Id = 1013, TypeOfOrder = SameDay, Address = Begin 101, Petah Tikva, Latitude = 32.088, Longitude = 34.8875, CustomerName = Nina Ronen, CustomerPhone = 050-215-8422, OrderOpeningTime = 25/07/2024 05:02:20, PackageDetails = Photography print envelope, Description = Check authenticity certificate before handover }
Order { Id = 1014, TypeOfOrder = Express, Address = Sderot Hen 5, Haifa, Latitude = 32.804, Longitude = 34.9896, CustomerName = Amir Ilan, CustomerPhone = 050-814-5309, OrderOpeningTime = 13/11/2024 12:31:19, PackageDetails = Limited edition art box, Description = Carefully packed for collector }
Order { Id = 1015, TypeOfOrder = Regular, Address = Ha'atzmaut 25, Bat Yam, Latitude = 32.021, Longitude = 34.7544, CustomerName = Dana Marciano, CustomerPhone = 050-144-2054, OrderOpeningTime = 12/07/2021 09:24:20, PackageDetails = Canvas painting - medium size, Description = Standard art delivery }
Order { Id = 1016, TypeOfOrder = Regular, Address = Modi'in, Latitude = 31.8989, Longitude = 35.0078, CustomerName = Jonathan Tzadok, CustomerPhone = 053-790-9089, OrderOpeningTime = 24/07/2023 06:18:52, PackageDetails = Fragile - framed artwork, Description = Handle with care - original artwork }
Order { Id = 1017, TypeOfOrder = Express, Address = Ashdod, Latitude = 31.8044, Longitude = 34.6553, CustomerName = Liad Baruch, CustomerPhone = 054-302-9601, OrderOpeningTime = 09/01/2025 09:10:31, PackageDetails = Sculpture package - heavy, Description = Urgent exhibition piece - deliver directly to gallery }
Order { Id = 1018, TypeOfOrder = SameDay, Address = Beersheba, Latitude = 31.2518, Longitude = 34.7915, CustomerName = Omer Amir, CustomerPhone = 058-861-5574, OrderOpeningTime = 22/07/2024 13:02:15, PackageDetails = Photography print envelope, Description = Check authenticity certificate before handover }
Order { Id = 1019, TypeOfOrder = Express, Address = Rehovot, Latitude = 31.8948, Longitude = 34.811, CustomerName = Ruth Hazan, CustomerPhone = 055-693-8578, OrderOpeningTime = 28/03/2021 12:19:37, PackageDetails = Limited edition art box, Description = Carefully packed for collector }
Order { Id = 1020, TypeOfOrder = SameDay, Address = Herzl 10, Tel Aviv, Latitude = 32.0675, Longitude = 34.7775, CustomerName = Eli Mor, CustomerPhone = 056-421-7308, OrderOpeningTime = 22/04/2024 06:02:52, PackageDetails = Canvas painting - medium size, Description = Standard art delivery }
Order { Id = 1021, TypeOfOrder = Express, Address = Jaffa Road 2, Jerusalem, Latitude = 31.778, Longitude = 35.2345, CustomerName = Sara Peretz, CustomerPhone = 056-646-5859, OrderOpeningTime = 13/03/2025 14:20:18, PackageDetails = Fragile - framed artwork, Description = Handle with care - original artwork }
Order { Id = 1022, TypeOfOrder = Regular, Address = Ha'arbaa 14, Herzliya, Latitude = 32.1632, Longitude = 34.8406, CustomerName = Ben Avital, CustomerPhone = 052-493-8645, OrderOpeningTime = 17/07/2022 13:50:30, PackageDetails = Sculpture package - heavy, Description = Urgent exhibition piece - deliver directly to gallery }
Order { Id = 1023, TypeOfOrder = Regular, Address = Begin 101, Petah Tikva, Latitude = 32.088, Longitude = 34.8875, CustomerName = Hila Levi, CustomerPhone = 053-990-9233, OrderOpeningTime = 27/07/2021 14:41:34, PackageDetails = Photography print envelope, Description = Check authenticity certificate before handover }
Order { Id = 1024, TypeOfOrder = Express, Address = Sderot Hen 5, Haifa, Latitude = 32.804, Longitude = 34.9896, CustomerName = Yoni Weiss, CustomerPhone = 052-429-5895, OrderOpeningTime = 03/10/2022 10:02:11, PackageDetails = Limited edition art box, Description = Carefully packed for collector }
Order { Id = 1025, TypeOfOrder = Express, Address = Ha'atzmaut 25, Bat Yam, Latitude = 32.021, Longitude = 34.7544, CustomerName = Nadav Regev, CustomerPhone = 056-604-8739, OrderOpeningTime = 24/03/2021 08:49:02, PackageDetails = Canvas painting - medium size, Description = Standard art delivery }
Order { Id = 1026, TypeOfOrder = Regular, Address = Modi'in, Latitude = 31.8989, Longitude = 35.0078, CustomerName = Tal Segal, CustomerPhone = 052-224-1119, OrderOpeningTime = 28/06/2021 13:13:07, PackageDetails = Fragile - framed artwork, Description = Handle with care - original artwork }
Order { Id = 1027, TypeOfOrder = Regular, Address = Ashdod, Latitude = 31.8044, Longitude = 34.6553, CustomerName = Naomi Klein, CustomerPhone = 055-530-1644, OrderOpeningTime = 08/03/2022 10:35:50, PackageDetails = Sculpture package - heavy, Description = Urgent exhibition piece - deliver directly to gallery }
Order { Id = 1028, TypeOfOrder = Regular, Address = Beersheba, Latitude = 31.2518, Longitude = 34.7915, CustomerName = Oren Goldstein, CustomerPhone = 053-941-5105, OrderOpeningTime = 20/10/2021 13:12:37, PackageDetails = Photography print envelope, Description = Check authenticity certificate before handover }
Order { Id = 1029, TypeOfOrder = Regular, Address = Rehovot, Latitude = 31.8948, Longitude = 34.811, CustomerName = Yael Mizrahi, CustomerPhone = 054-559-9156, OrderOpeningTime = 24/03/2022 02:58:18, PackageDetails = Limited edition art box, Description = Carefully packed for collector }
Order { Id = 1030, TypeOfOrder = Regular, Address = Herzl 10, Tel Aviv, Latitude = 32.0675, Longitude = 34.7775, CustomerName = Ariel Rubin, CustomerPhone = 052-614-4621, OrderOpeningTime = 02/10/2021 05:23:48, PackageDetails = Canvas painting - medium size, Description = Standard art delivery }
Order { Id = 1031, TypeOfOrder = SameDay, Address = Jaffa Road 2, Jerusalem, Latitude = 31.778, Longitude = 35.2345, CustomerName = Noa Shahar, CustomerPhone = 056-388-6525, OrderOpeningTime = 17/05/2025 02:49:53, PackageDetails = Fragile - framed artwork, Description = Handle with care - original artwork }
Order { Id = 1032, TypeOfOrder = Express, Address = Ha'arbaa 14, Herzliya, Latitude = 32.1632, Longitude = 34.8406, CustomerName = Idan Azulay, CustomerPhone = 055-267-2324, OrderOpeningTime = 08/03/2022 14:29:16, PackageDetails = Sculpture package - heavy, Description = Urgent exhibition piece - deliver directly to gallery }
Order { Id = 1033, TypeOfOrder = SameDay, Address = Begin 101, Petah Tikva, Latitude = 32.088, Longitude = 34.8875, CustomerName = Romi Halevi, CustomerPhone = 058-756-8414, OrderOpeningTime = 01/04/2025 06:12:19, PackageDetails = Photography print envelope, Description = Check authenticity certificate before handover }
Order { Id = 1034, TypeOfOrder = Express, Address = Sderot Hen 5, Haifa, Latitude = 32.804, Longitude = 34.9896, CustomerName = Erez Dayan, CustomerPhone = 050-841-7341, OrderOpeningTime = 02/08/2024 02:20:22, PackageDetails = Limited edition art box, Description = Carefully packed for collector }
Order { Id = 1035, TypeOfOrder = SameDay, Address = Ha'atzmaut 25, Bat Yam, Latitude = 32.021, Longitude = 34.7544, CustomerName = Galit Ashkenazi, CustomerPhone = 053-431-8757, OrderOpeningTime = 06/11/2025 04:24:58, PackageDetails = Canvas painting - medium size, Description = Standard art delivery }
Order { Id = 1036, TypeOfOrder = Regular, Address = Modi'in, Latitude = 31.8989, Longitude = 35.0078, CustomerName = Doron Koren, CustomerPhone = 050-578-1603, OrderOpeningTime = 16/05/2022 10:50:27, PackageDetails = Fragile - framed artwork, Description = Handle with care - original artwork }
Order { Id = 1037, TypeOfOrder = Express, Address = Ashdod, Latitude = 31.8044, Longitude = 34.6553, CustomerName = Liat Morad, CustomerPhone = 050-614-1016, OrderOpeningTime = 01/12/2022 10:02:41, PackageDetails = Sculpture package - heavy, Description = Urgent exhibition piece - deliver directly to gallery }
Order { Id = 1038, TypeOfOrder = Express, Address = Beersheba, Latitude = 31.2518, Longitude = 34.7915, CustomerName = Ori Katz, CustomerPhone = 052-995-5250, OrderOpeningTime = 25/05/2024 13:18:53, PackageDetails = Photography print envelope, Description = Check authenticity certificate before handover }
Order { Id = 1039, TypeOfOrder = Regular, Address = Rehovot, Latitude = 31.8948, Longitude = 34.811, CustomerName = Ella Shemesh, CustomerPhone = 055-405-6645, OrderOpeningTime = 12/09/2024 14:37:10, PackageDetails = Limited edition art box, Description = Carefully packed for collector }
Order { Id = 1040, TypeOfOrder = Regular, Address = Herzl 10, Tel Aviv, Latitude = 32.0675, Longitude = 34.7775, CustomerName = Ron Biton, CustomerPhone = 055-577-4786, OrderOpeningTime = 11/11/2023 15:34:43, PackageDetails = Canvas painting - medium size, Description = Standard art delivery }
Order { Id = 1041, TypeOfOrder = Express, Address = Jaffa Road 2, Jerusalem, Latitude = 31.778, Longitude = 35.2345, CustomerName = Talia Oren, CustomerPhone = 057-247-6344, OrderOpeningTime = 01/05/2024 14:13:28, PackageDetails = Fragile - framed artwork, Description = Handle with care - original artwork }
Order { Id = 1042, TypeOfOrder = Regular, Address = Ha'arbaa 14, Herzliya, Latitude = 32.1632, Longitude = 34.8406, CustomerName = Gadi Ben-Haim, CustomerPhone = 056-301-2521, OrderOpeningTime = 27/12/2023 09:30:12, PackageDetails = Sculpture package - heavy, Description = Urgent exhibition piece - deliver directly to gallery }
Order { Id = 1043, TypeOfOrder = SameDay, Address = Begin 101, Petah Tikva, Latitude = 32.088, Longitude = 34.8875, CustomerName = Eden Cohen, CustomerPhone = 056-489-1825, OrderOpeningTime = 07/12/2021 03:05:52, PackageDetails = Photography print envelope, Description = Check authenticity certificate before handover }
Order { Id = 1044, TypeOfOrder = SameDay, Address = Sderot Hen 5, Haifa, Latitude = 32.804, Longitude = 34.9896, CustomerName = Noy Levi, CustomerPhone = 052-871-6909, OrderOpeningTime = 23/11/2021 12:24:33, PackageDetails = Limited edition art box, Description = Carefully packed for collector }
Order { Id = 1045, TypeOfOrder = Express, Address = Ha'atzmaut 25, Bat Yam, Latitude = 32.021, Longitude = 34.7544, CustomerName = Roy Shaked, CustomerPhone = 053-460-5139, OrderOpeningTime = 10/08/2025 12:06:29, PackageDetails = Canvas painting - medium size, Description = Standard art delivery }
Order { Id = 1046, TypeOfOrder = Regular, Address = Modi'in, Latitude = 31.8989, Longitude = 35.0078, CustomerName = Alon Baruch, CustomerPhone = 058-779-4385, OrderOpeningTime = 08/02/2024 10:36:44, PackageDetails = Fragile - framed artwork, Description = Handle with care - original artwork }
Order { Id = 1047, TypeOfOrder = Express, Address = Ashdod, Latitude = 31.8044, Longitude = 34.6553, CustomerName = Michal Dahan, CustomerPhone = 050-783-5810, OrderOpeningTime = 31/07/2023 04:01:50, PackageDetails = Sculpture package - heavy, Description = Urgent exhibition piece - deliver directly to gallery }
Order { Id = 1048, TypeOfOrder = Express, Address = Beersheba, Latitude = 31.2518, Longitude = 34.7915, CustomerName = Yarden Golan, CustomerPhone = 058-587-8464, OrderOpeningTime = 06/02/2025 14:19:29, PackageDetails = Photography print envelope, Description = Check authenticity certificate before handover }
Order { Id = 1049, TypeOfOrder = SameDay, Address = Rehovot, Latitude = 31.8948, Longitude = 34.811, CustomerName = Nadav Tal, CustomerPhone = 056-777-7973, OrderOpeningTime = 17/07/2021 12:48:43, PackageDetails = Limited edition art box, Description = Carefully packed for collector }

-- Deliveries --
Delivery { Id = 1000, OrderId = 1044, CourierId = 262581120, TypeOfOrder = SameDay, DeliveryStartTime = 17/11/2024 22:55:33, ActualDistance = 82.26031301295912, OrderEndStatus = Delivered, DeliveryEndTime = 18/11/2024 00:43:33 }
Delivery { Id = 1001, OrderId = 1037, CourierId = 344926647, TypeOfOrder = Express, DeliveryStartTime = 02/12/2024 08:58:41, ActualDistance = 33.437552023096465, OrderEndStatus = Cancelled, DeliveryEndTime = 02/12/2024 11:08:41 }
Delivery { Id = 1002, OrderId = 1042, CourierId = 956087862, TypeOfOrder = Regular, DeliveryStartTime = 14/07/2024 06:40:12, ActualDistance = 10.28069354356626, OrderEndStatus = Refused, DeliveryEndTime = 14/07/2024 07:03:12 }
Delivery { Id = 1003, OrderId = 1048, CourierId = 262581120, TypeOfOrder = Express, DeliveryStartTime = 06/02/2025 17:03:29, ActualDistance = 92.6855173262227, OrderEndStatus = Delivered, DeliveryEndTime = 06/02/2025 19:17:29 }
Delivery { Id = 1004, OrderId = 1046, CourierId = 956087862, TypeOfOrder = Regular, DeliveryStartTime = 11/08/2025 16:02:44, ActualDistance = 29.729690756933437, OrderEndStatus = Delivered, DeliveryEndTime = 11/08/2025 17:52:44 }
Delivery { Id = 1005, OrderId = 1032, CourierId = 543020197, TypeOfOrder = Express, DeliveryStartTime = 03/07/2022 09:31:16, ActualDistance = 10.28069354356626, OrderEndStatus = InviterNotFound, DeliveryEndTime = 03/07/2022 10:45:16 }
Delivery { Id = 1006, OrderId = 1021, CourierId = 956087862, TypeOfOrder = Express, DeliveryStartTime = 15/07/2025 04:08:18, ActualDistance = 54.705175646146685, OrderEndStatus = Failed, DeliveryEndTime = 15/07/2025 06:05:18 }
Delivery { Id = 1007, OrderId = 1049, CourierId = 694180588, TypeOfOrder = SameDay, DeliveryStartTime = 27/09/2023 01:46:43, ActualDistance = 21.36088640832011, OrderEndStatus = Failed, DeliveryEndTime = 27/09/2023 04:39:43 }
Delivery { Id = 1008, OrderId = 1036, CourierId = 956087862, TypeOfOrder = Regular, DeliveryStartTime = 27/09/2024 09:23:27, ActualDistance = 29.729690756933437, OrderEndStatus = InviterNotFound, DeliveryEndTime = 27/09/2024 10:38:27 }
Delivery { Id = 1009, OrderId = 1028, CourierId = 956087862, TypeOfOrder = Regular, DeliveryStartTime = 21/07/2024 22:05:37, ActualDistance = 92.6855173262227, OrderEndStatus = Cancelled, DeliveryEndTime = 21/07/2024 23:03:37 }
Delivery { Id = 1010, OrderId = 1026, CourierId = 483649597, TypeOfOrder = Regular, DeliveryStartTime = 04/03/2024 02:36:07, ActualDistance = 29.729690756933437, OrderEndStatus = Refused, DeliveryEndTime = 04/03/2024 03:42:07 }
Delivery { Id = 1011, OrderId = 1022, CourierId = 323019443, TypeOfOrder = Regular, DeliveryStartTime = 01/05/2024 23:16:30, ActualDistance = 10.28069354356626, OrderEndStatus = Failed, DeliveryEndTime = 02/05/2024 02:08:30 }
Delivery { Id = 1012, OrderId = 1034, CourierId = 483649597, TypeOfOrder = Express, DeliveryStartTime = 06/06/2025 12:41:22, ActualDistance = 82.26031301295912, OrderEndStatus = Cancelled, DeliveryEndTime = 06/06/2025 13:09:22 }
Delivery { Id = 1013, OrderId = 1008, CourierId = 483649597, TypeOfOrder = Regular, DeliveryStartTime = 08/05/2024 13:23:36, ActualDistance = 92.6855173262227, OrderEndStatus = Cancelled, DeliveryEndTime = 08/05/2024 15:52:36 }
Delivery { Id = 1014, OrderId = 1017, CourierId = 483649597, TypeOfOrder = Express, DeliveryStartTime = 16/01/2025 01:37:31, ActualDistance = 33.437552023096465, OrderEndStatus = Failed, DeliveryEndTime = 16/01/2025 02:37:31 }
Delivery { Id = 1015, OrderId = 1039, CourierId = 141244895, TypeOfOrder = Regular, DeliveryStartTime = 29/04/2025 08:19:10, ActualDistance = 21.36088640832011, OrderEndStatus = InviterNotFound, DeliveryEndTime = 29/04/2025 09:28:10 }
Delivery { Id = 1016, OrderId = 1047, CourierId = 956087862, TypeOfOrder = Express, DeliveryStartTime = 10/09/2024 23:34:50, ActualDistance = 33.437552023096465, OrderEndStatus = Delivered, DeliveryEndTime = 10/09/2024 23:59:50 }
Delivery { Id = 1017, OrderId = 1024, CourierId = 262581120, TypeOfOrder = Express, DeliveryStartTime = 08/01/2025 12:35:11, ActualDistance = 82.26031301295912, OrderEndStatus = Failed, DeliveryEndTime = 08/01/2025 13:30:11 }
Delivery { Id = 1018, OrderId = 1001, CourierId = 262581120, TypeOfOrder = SameDay, DeliveryStartTime = 07/09/2025 13:57:20, ActualDistance = 54.705175646146685, OrderEndStatus = InviterNotFound, DeliveryEndTime = 07/09/2025 14:43:20 }
Delivery { Id = 1019, OrderId = 1014, CourierId = 956087862, TypeOfOrder = Express, DeliveryStartTime = 08/03/2025 07:00:19, ActualDistance = 82.26031301295912, OrderEndStatus = InviterNotFound, DeliveryEndTime = 08/03/2025 09:03:19 }
Delivery { Id = 1020, OrderId = 1019, CourierId = 956087862, TypeOfOrder = Express, DeliveryStartTime = 26/09/2025 14:17:37, ActualDistance = 21.36088640832011, OrderEndStatus = , DeliveryEndTime =  }
Delivery { Id = 1021, OrderId = 1002, CourierId = 323019443, TypeOfOrder = SameDay, DeliveryStartTime = 09/11/2025 06:12:38, ActualDistance = 10.28069354356626, OrderEndStatus = , DeliveryEndTime =  }
Delivery { Id = 1022, OrderId = 1029, CourierId = 141244895, TypeOfOrder = Regular, DeliveryStartTime = 05/11/2025 02:16:18, ActualDistance = 21.36088640832011, OrderEndStatus = , DeliveryEndTime =  }
Delivery { Id = 1023, OrderId = 1015, CourierId = 543020197, TypeOfOrder = Regular, DeliveryStartTime = 21/10/2022 23:59:20, ActualDistance = 7.601862445755016, OrderEndStatus = , DeliveryEndTime =  }
Delivery { Id = 1024, OrderId = 1031, CourierId = 483649597, TypeOfOrder = SameDay, DeliveryStartTime = 21/07/2025 23:06:53, ActualDistance = 54.705175646146685, OrderEndStatus = , DeliveryEndTime =  }
Delivery { Id = 1025, OrderId = 1005, CourierId = 391874673, TypeOfOrder = Regular, DeliveryStartTime = 11/02/2023 04:32:25, ActualDistance = 7.601862445755016, OrderEndStatus = , DeliveryEndTime =  }

--- END OF LIST ---

--- MAIN MENU ---
0: Exit
1: Courier Menu
2: Order Menu
3: Delivery Menu
4: Config Menu
5: Initialize Data (Reset and Create)
6: Reset All Data (Clear Lists & Config)
7: List All Data
Enter your choice: 0
Exiting program. Goodbye!

C:\Users\1\source\repos\dotNet5786_2540_1617\bin\DalTest.exe (process 21080) exited with code 0 (0x0).
Press any key to close this window . . .
 */