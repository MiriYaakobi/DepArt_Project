using Dal;
using DalApi;
using DO;
using System;
using System.Collections.Generic;
using static DalTest.Program;

namespace DalTest;

internal class Program
{
    private static ICourier? s_dalCourier = new CourierImplementation();
    private static IOrder? s_dalOrder = new OrderImplementation();
    private static IDelivery? s_dalDelivery = new DeliveryImplementation();
    private static IConfig? s_dalConfig = new ConfigImplementation();

    public enum CrudMenu
    {
        Exit,
        Create,
        Read,
        ReadAll,
        Update,
        Delete,
        DeleteAll
    }
    public enum MainMenu
    {
        Exit,
        ShowCourier,
        ShowOrder,
        ShowDelivery,
        Initialization,
        ShowAllData,
        ShowConfig,
        ResetData
    }
    public enum ConfigMenu
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

    private static int ReadInt(string prompt)
    {
        Console.Write(prompt);
        if (!int.TryParse(Console.ReadLine(), out int value))
            throw new FormatException("Input is not a valid integer.");
        return value;
    }

    private static double ReadDouble(string prompt)
    {
        Console.Write(prompt);
        if (!double.TryParse(Console.ReadLine(), out double value))
            throw new FormatException("Input is not a valid double.");
        return value;
    }

    private static bool ReadBool(string prompt)
    {
        Console.Write(prompt);
        if (!bool.TryParse(Console.ReadLine(), out bool value))
            throw new FormatException("Input is not a valid boolean (true/false).");
        return value;
    }

    private static DateTime ReadDateTime(string prompt)
    {
        Console.Write(prompt);
        if (!DateTime.TryParse(Console.ReadLine(), out DateTime dt))
            throw new FormatException("DateTime is invalid!");
        return dt;
    }

    private static TimeSpan ReadTimeSpan(string prompt)
    {
        Console.Write(prompt);
        if (!TimeSpan.TryParse(Console.ReadLine(), out TimeSpan ts))
            throw new FormatException("TimeSpan is invalid! (e.g., 01:30:00)");
        return ts;
    }

    private static string ReadNonEmpty(string prompt)
    {
        Console.Write(prompt);
        var s = Console.ReadLine() ?? "";
        if (string.IsNullOrWhiteSpace(s))
            throw new FormatException("Input is empty.");
        return s;
    }

    private static string ReadPossiblyEmpty(string prompt)
    {
        Console.Write(prompt);
        return Console.ReadLine() ?? "";
    }

    private static int? ReadIntOrEmpty(string prompt)
    {
        Console.Write(prompt);
        var s = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(s)) return null;
        if (!int.TryParse(s, out int v)) throw new FormatException("Input is not a valid integer.");
        return v;
    }

    private static double? ReadDoubleOrEmpty(string prompt)
    {
        Console.Write(prompt);
        var s = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(s)) return null;
        if (!double.TryParse(s, out double v)) throw new FormatException("Input is not a valid double.");
        return v;
    }

    private static bool? ReadBoolOrEmpty(string prompt)
    {
        Console.Write(prompt);
        var s = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(s)) return null;
        if (!bool.TryParse(s, out bool v)) throw new FormatException("Input is not a valid boolean.");
        return v;
    }

    private static DateTime? ReadDateTimeOrEmpty(string prompt)
    {
        Console.Write(prompt);
        var s = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(s)) return null;
        if (!DateTime.TryParse(s, out DateTime dt)) throw new FormatException("DateTime is invalid!");
        return dt;
    }

    private static TimeSpan? ReadTimeSpanOrEmpty(string prompt)
    {
        Console.Write(prompt);
        var s = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(s)) return null;
        if (!TimeSpan.TryParse(s, out TimeSpan ts)) throw new FormatException("TimeSpan is invalid!");
        return ts;
    }

    private static DO.OrderType ReadOrderType(string prompt)
    {
        Console.Write(prompt);
        var s = Console.ReadLine();
        if (Enum.TryParse<DO.OrderType>(s, true, out var val)) return val;
        throw new FormatException("Invalid OrderType. Allowed: Car/Motorcycle/Bicycle/Walking");
    }

    private static DO.OrderType? ReadOrderTypeOrEmpty(string prompt)
    {
        Console.Write(prompt);
        var s = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(s)) return null;
        if (Enum.TryParse<DO.OrderType>(s, true, out var val)) return val;
        throw new FormatException("Invalid OrderType. Allowed: Car/Motorcycle/Bicycle/Walking");
    }

    static void Main(string[] args)
    {
        try
        {
            Console.WriteLine("Welcome to the Delivery Management System!\n");
            showMainMenu();
            Enum.TryParse(Console.ReadLine() ?? "0", out MainMenu choice);
            while (choice != MainMenu.Exit)
            {
                switch (choice)
                {
                    case MainMenu.ShowCourier:
                        showCrud("Courier");
                        Enum.TryParse(Console.ReadLine() ?? "0", out CrudMenu courierChoice);
                        while (courierChoice != CrudMenu.Exit)
                        {
                            switch (courierChoice)
                            {
                                case CrudMenu.Create:
                                    createCourier();
                                    break;
                                case CrudMenu.Read:
                                    printCourier();
                                    break;
                                case CrudMenu.ReadAll:
                                    printAllCouriers();
                                    break;
                                case CrudMenu.Update:
                                    updateCourier();
                                    break;
                                case CrudMenu.Delete:
                                    deleteCourier();
                                    break;
                                case CrudMenu.DeleteAll:
                                    deleteAllCouriers();
                                    break;
                                default:
                                    Console.WriteLine("Invalid choice. Please try again.");
                                    break;
                            }
                            showCrud("Courier");
                            Enum.TryParse(Console.ReadLine() ?? "0", out courierChoice);
                        }
                        break;
                    case MainMenu.ShowOrder:
                        showCrud("Order");
                        Enum.TryParse(Console.ReadLine() ?? "0", out CrudMenu orderChoice);
                        while (orderChoice != CrudMenu.Exit)
                        {
                            switch (orderChoice)
                            {
                                case CrudMenu.Create:
                                    createOrder();
                                    break;
                                case CrudMenu.Read:
                                    printOrder();
                                    break;
                                case CrudMenu.ReadAll:
                                    printAllOrders();
                                    break;
                                case CrudMenu.Update:
                                    updateOrder();
                                    break;
                                case CrudMenu.Delete:
                                    deleteOrder();
                                    break;
                                case CrudMenu.DeleteAll:
                                    deleteAllOrders();
                                    break;
                                default:
                                    Console.WriteLine("Invalid choice. Please try again.");
                                    break;
                            }
                            showCrud("Order");
                            Enum.TryParse(Console.ReadLine() ?? "0", out orderChoice);
                        }
                        break;
                    case MainMenu.ShowDelivery:
                        showCrud("Delivery");
                        Enum.TryParse(Console.ReadLine() ?? "0", out CrudMenu deliveryChoice);
                        while (deliveryChoice != CrudMenu.Exit)
                        {
                            switch (deliveryChoice)
                            {
                                case CrudMenu.Create:
                                    createDelivery();
                                    break;
                                case CrudMenu.Read:
                                    printDelivery();
                                    break;
                                case CrudMenu.ReadAll:
                                    printAllDeliveries();
                                    break;
                                case CrudMenu.Update:
                                    updateDelivery();
                                    break;
                                case CrudMenu.Delete:
                                    deleteDelivery();
                                    break;
                                case CrudMenu.DeleteAll:
                                    deleteAllDeliveries();
                                    break;
                                default:
                                    Console.WriteLine("Invalid choice. Please try again.");
                                    break;
                            }
                            showCrud("Delivery");
                            Enum.TryParse(Console.ReadLine() ?? "0", out deliveryChoice);
                        }
                        break;
                    case MainMenu.Initialization:
                        initializationAllData();
                        break;
                    case MainMenu.ShowAllData:
                        showAllData();
                        break;
                    case MainMenu.ShowConfig:
                        showCONFIG();
                        Enum.TryParse(Console.ReadLine() ?? "0", out ConfigMenu configChoice);
                        while (configChoice != ConfigMenu.Exit)
                        {
                            switch (configChoice)
                            {
                                case ConfigMenu.AddMinute:
                                    s_dalConfig.Clock = s_dalConfig.Clock.AddMinutes(1);
                                    Console.WriteLine($"Clock updated to: {s_dalConfig.Clock}");
                                    break;
                                case ConfigMenu.AddHour:
                                    s_dalConfig.Clock = s_dalConfig.Clock.AddHours(1);
                                    Console.WriteLine($"Clock updated to: {s_dalConfig.Clock}");
                                    break;
                                case ConfigMenu.AddDay:
                                    s_dalConfig.Clock = s_dalConfig.Clock.AddDays(1);
                                    Console.WriteLine($"Clock updated to: {s_dalConfig.Clock}");
                                    break;
                                case ConfigMenu.AddWeek:
                                    s_dalConfig.Clock = s_dalConfig.Clock.AddDays(7);
                                    Console.WriteLine($"Clock updated to: {s_dalConfig.Clock}");
                                    break;
                                case ConfigMenu.AddMonth:
                                    s_dalConfig.Clock = s_dalConfig.Clock.AddMonths(1);
                                    Console.WriteLine($"Clock updated to: {s_dalConfig.Clock}");
                                    break;
                                case ConfigMenu.AddYear:
                                    s_dalConfig.Clock = s_dalConfig.Clock.AddYears(1);
                                    Console.WriteLine($"Clock updated to: {s_dalConfig.Clock}");
                                    break;
                                case ConfigMenu.ShowCurrentClock:
                                    Console.WriteLine($"Current Clock: {s_dalConfig.Clock}");
                                    break;
                                case ConfigMenu.UpdateVariable:
                                    updateVariable();
                                    break;
                                case ConfigMenu.ShowVariable:
                                    showVariable();
                                    Console.WriteLine(" ");
                                    break;
                                case ConfigMenu.ResetAllConfig:
                                    s_dalConfig.Reset();
                                    Console.WriteLine("All configuration reset to default values.");
                                    break;
                                case ConfigMenu.Exit:
                                    break;
                                default:
                                    Console.WriteLine("Invalid choice. Please try again.");
                                    break;
                            }
                            showCONFIG();
                            Enum.TryParse(Console.ReadLine() ?? "0", out configChoice);
                        }
                        break;
                    case MainMenu.ResetData:
                        resetConfigAndData();
                        break;
                    case MainMenu.Exit:
                        Console.WriteLine("EXITing...");
                        return;
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
                showMainMenu();
                Enum.TryParse(Console.ReadLine() ?? "0", out choice);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
    private static void showMainMenu()
    {
        Console.WriteLine("Main Menu:");
        Console.WriteLine("1. Show Courier Menu");
        Console.WriteLine("2. Show Order Menu");
        Console.WriteLine("3. Show Delivery Menu");
        Console.WriteLine("4. Initialization All Data");
        Console.WriteLine("5. Show All Data");
        Console.WriteLine("6. Show Config Menu");
        Console.WriteLine("7. Reset Config And Data");
        Console.WriteLine("0. EXIT");
    }


    private static void showCrud(string entityName)
    {
        Console.WriteLine($"{entityName} Menu:");
        Console.WriteLine("1. Add");
        Console.WriteLine("2. Read");
        Console.WriteLine("3. Read All");
        Console.WriteLine("4. Update");
        Console.WriteLine("5. Delete");
        Console.WriteLine("6. Delete All");
        Console.WriteLine("0. EXIT");
    }

    private static void initializationAllData()
    {
        Initialization.Do(s_dalConfig, s_dalCourier, s_dalOrder, s_dalDelivery);
    }

    private static void createCourier()
    {
        try
        {
            Console.WriteLine("=== Create Courier ===");
            int id = ReadInt("Id: ");
            string name = ReadNonEmpty("Name: ");
            string phone = ReadNonEmpty("PhoneNumber: ");
            string email = ReadNonEmpty("Email: ");
            string password = ReadNonEmpty("Password: ");
            bool active = ReadBool("Active (true/false): ");
            var orderType = ReadOrderType("OrderType (Car/Motorcycle/Bicycle/Walking): ");
            DateTime startWorking = ReadDateTime("StartWorking (e.g., 2025-11-05 13:45): ");
            double? personalMaxDistance = ReadDoubleOrEmpty("PersonalMaxDistance (empty = null): ");

            var courier = new DO.Courier(
                Id: id,
                Name: name,
                Phone: phone,
                Email: email,
                Password: password,
                IsActive: active,
                TypeOfDelivery: (Enum)orderType,
                StartWorkTime: startWorking,
                MaxDist: personalMaxDistance
            );

            s_dalCourier!.Create(courier);
            Console.WriteLine("Courier created.");
        }
        catch (Exception ex) { Console.WriteLine(ex); }
    }

    private static void printCourier()
    {
        try
        {
            Console.WriteLine("=== Read Courier ===");
            int id = ReadInt("Id: ");
            var c = s_dalCourier!.Read(id);
            if (c is null) { Console.WriteLine("Courier not found."); return; }
            Console.WriteLine(c);
        }
        catch (Exception ex) { Console.WriteLine(ex); }
    }

    private static void printAllCouriers()
    {
        try
        {
            Console.WriteLine("=== All Couriers ===");
            foreach (var c in s_dalCourier!.ReadAll())
                Console.WriteLine(c);
        }
        catch (Exception ex) { Console.WriteLine(ex); }
    }

    private static void updateCourier()
    {
        try
        {
            Console.WriteLine("=== Update Courier ===");
            int id = ReadInt("Id to update: ");
            var existing = s_dalCourier!.Read(id);
            if (existing is null) { Console.WriteLine("Courier not found."); return; }
            Console.WriteLine("Current:"); Console.WriteLine(existing);

            string name = ReadPossiblyEmpty("Name (empty=keep): ");
            string phone = ReadPossiblyEmpty("PhoneNumber (empty=keep): ");
            string email = ReadPossiblyEmpty("Email (empty=keep): ");
            string password = ReadPossiblyEmpty("Password (empty=keep): ");
            bool? active = ReadBoolOrEmpty("Active (true/false, empty=keep): ");
            var ot = ReadOrderTypeOrEmpty("OrderType (Car/Motorcycle/Bicycle/Walking, empty=keep): ");
            var startW = ReadDateTimeOrEmpty("StartWorking (empty=keep): ");
            var pmd = ReadDoubleOrEmpty("PersonalMaxDistance (empty=keep): ");

            var updated = existing with
            {
                Name = string.IsNullOrWhiteSpace(name) ? existing.Name : name,
                Phone = string.IsNullOrWhiteSpace(phone) ? existing.Phone : phone,
                Email = string.IsNullOrWhiteSpace(email) ? existing.Email : email,
                Password = string.IsNullOrWhiteSpace(password) ? existing.Password : password,
                IsActive = active ?? existing.IsActive,
                TypeOfDelivery = (DeliveryType)(ot.HasValue ? (Enum)ot.Value : existing.TypeOfDelivery),
                StartWorkTime = startW ?? existing.StartWorkTime,
                MaxDist = pmd.HasValue ? pmd : existing.MaxDist
            };

            s_dalCourier.Update(updated);
            Console.WriteLine("Courier updated.");
        }
        catch (Exception ex) { Console.WriteLine(ex); }
    }

    private static void deleteCourier()
    {
        try
        {
            Console.WriteLine("=== Delete Courier ===");
            int id = ReadInt("Id: ");
            s_dalCourier!.Delete(id);
            Console.WriteLine("Courier deleted.");
        }
        catch (Exception ex) { Console.WriteLine(ex); }
    }

    private static void deleteAllCouriers()
    {
        try
        {
            Console.WriteLine("=== Delete All Couriers ===");
            s_dalCourier!.DeleteAll();
            Console.WriteLine("All Couriers deleted.");
        }
        catch (Exception ex) { Console.WriteLine(ex); }
    }

    private static void createOrder()
    {
        try
        {
            Console.WriteLine("=== Create Order ===");
            var orderType = ReadOrderType("OrderType (Car/Motorcycle/Bicycle/Walking): ");
            string address = ReadNonEmpty("Address: ");
            double latitude = ReadDouble("Latitude: ");
            double longitude = ReadDouble("Longitude: ");
            string customerName = ReadNonEmpty("CustomerName: ");
            string customerPhone = ReadNonEmpty("CustomerPhone: ");
            double volume = ReadDouble("Volume: ");
            double weight = ReadDouble("Weight: ");
            bool fragile = ReadBool("Fragile (true/false): ");
            DateTime openedAt = ReadDateTime("OpenedAt: ");
            string? desc = ReadPossiblyEmpty("Description (empty=null): ");
            if (string.IsNullOrWhiteSpace(desc)) desc = null;

            var order = new DO.Order(
                Id: 0, 
                TypeOfDelivery: (Enum)orderType,
                Address: address,
                Latitude: latitude,
                Longitude: longitude,
                CustomerName: customerName,
                CustomerPhone: customerPhone,
                Volume: volume,
                Weight: weight,
                Fragile: fragile,
                OpenedAt: openedAt,
                Description: desc
            );

            s_dalOrder!.Create(order);
            Console.WriteLine("Order created.");
        }
        catch (Exception ex) { Console.WriteLine(ex); }
    }

    private static void printOrder()
    {
        try
        {
            Console.WriteLine("=== Read Order ===");
            int id = ReadInt("Id: ");
            var o = s_dalOrder!.Read(id);
            if (o is null) { Console.WriteLine("Order not found."); return; }
            Console.WriteLine(o);
        }
        catch (Exception ex) { Console.WriteLine(ex); }
    }

    private static void printAllOrders()
    {
        try
        {
            Console.WriteLine("=== All Orders ===");
            foreach (var o in s_dalOrder!.ReadAll())
                Console.WriteLine(o);
        }
        catch (Exception ex) { Console.WriteLine(ex); }
    }

    private static void updateOrder()
    {
        try
        {
            Console.WriteLine("=== Update Order ===");
            int id = ReadInt("Id to update: ");
            var existing = s_dalOrder!.Read(id);
            if (existing is null) { Console.WriteLine("Order not found."); return; }
            Console.WriteLine("Current:"); Console.WriteLine(existing);

            var ot = ReadOrderTypeOrEmpty("OrderType (Car/Motorcycle/Bicycle/Walking, empty=keep): ");
            string address = ReadPossiblyEmpty("Address (empty=keep): ");
            var lat = ReadDoubleOrEmpty("Latitude (empty=keep): ");
            var lon = ReadDoubleOrEmpty("Longitude (empty=keep): ");
            string custName = ReadPossiblyEmpty("CustomerName (empty=keep): ");
            string custPhone = ReadPossiblyEmpty("CustomerPhone (empty=keep): ");
            var vol = ReadDoubleOrEmpty("Volume (empty=keep): ");
            var weight = ReadDoubleOrEmpty("Weight (empty=keep): ");
            var frag = ReadBoolOrEmpty("Fragile (true/false, empty=keep): ");
            var opened = ReadDateTimeOrEmpty("OpenedAt (empty=keep): ");
            string desc = ReadPossiblyEmpty("Description (empty=keep, use '-' to set null): ");
            string? newDesc = desc switch
            {
                "" => existing.Description,
                "-" => null,
                _ => desc
            };

            var updated = existing with
            {
                OrderType = ot.HasValue ? (Enum)ot.Value : existing.OrderType,
                Address = string.IsNullOrWhiteSpace(address) ? existing.Address : address,
                Latitude = lat ?? existing.Latitude,
                Longitude = lon ?? existing.Longitude,
                CustomerName = string.IsNullOrWhiteSpace(custName) ? existing.CustomerName : custName,
                CustomerPhone = string.IsNullOrWhiteSpace(custPhone) ? existing.CustomerPhone : custPhone,
                Volume = vol ?? existing.Volume,
                Weight = weight ?? existing.Weight,
                Fragile = frag ?? existing.Fragile,
                OpenedAt = opened ?? existing.OpenedAt,
                Description = newDesc
            };

            s_dalOrder.Update(updated);
            Console.WriteLine("Order updated.");
        }
        catch (Exception ex) { Console.WriteLine(ex); }
    }

    private static void deleteOrder()
    {
        try
        {
            Console.WriteLine("=== Delete Order ===");
            int id = ReadInt("Id: ");
            s_dalOrder!.Delete(id);
            Console.WriteLine("Order deleted.");
        }
        catch (Exception ex) { Console.WriteLine(ex); }
    }

    private static void deleteAllOrders()
    {
        try
        {
            Console.WriteLine("=== Delete All Orders ===");
            s_dalOrder!.DeleteAll();
            Console.WriteLine("All Orders deleted.");
        }
        catch (Exception ex) { Console.WriteLine(ex); }
    }

    private static void createDelivery()
    {
        try
        {
            Console.WriteLine("=== Create Delivery ===");
            int orderId = ReadInt("OrderId: ");
            int courierId = ReadInt("CourierId: ");
            var orderType = ReadOrderType("OrderType (Car/Motorcycle/Bicycle/Walking): ");
            DateTime startOrder = ReadDateTime("StartOrder: ");
            double? actualDistance = ReadDoubleOrEmpty("ActualDistance (empty=null): ");
            var typeEnd = ReadOrderTypeOrEmpty("TypeEndOrder (Car/Motorcycle/Bicycle/Walking, empty=null): ");
            var timeEnd = ReadDateTimeOrEmpty("TimeEndOrder (empty=null): ");

            var delivery = new DO.Delivery(
                Id: 0,
                OrderId: orderId,
                CourierId: courierId,
                OrderType: (Enum)orderType,
                StartOrder: startOrder,
                ActualDistance: actualDistance,
                TypeEndOrder: typeEnd.HasValue ? (Enum?)typeEnd.Value : null,
                TimeEndOrder: timeEnd
            );

            s_dalDelivery!.Create(delivery);
            Console.WriteLine("Delivery created.");
        }
        catch (Exception ex) { Console.WriteLine(ex); }
    }

    private static void printDelivery()
    {
        try
        {
            Console.WriteLine("=== Read Delivery ===");
            int id = ReadInt("Id: ");
            var d = s_dalDelivery!.Read(id);
            if (d is null) { Console.WriteLine("Delivery not found."); return; }
            Console.WriteLine(d);
        }
        catch (Exception ex) { Console.WriteLine(ex); }
    }

    private static void printAllDeliveries()
    {
        try
        {
            Console.WriteLine("=== All Deliveries ===");
            foreach (var d in s_dalDelivery!.ReadAll())
                Console.WriteLine(d);
        }
        catch (Exception ex) { Console.WriteLine(ex); }
    }

    private static void updateDelivery()
    {
        try
        {
            Console.WriteLine("=== Update Delivery ===");
            int id = ReadInt("Id to update: ");
            var existing = s_dalDelivery!.Read(id);
            if (existing is null) { Console.WriteLine("Delivery not found."); return; }
            Console.WriteLine("Current:"); Console.WriteLine(existing);

            var orderId = ReadIntOrEmpty("OrderId (empty=keep): ");
            var courierId = ReadIntOrEmpty("CourierId (empty=keep): ");
            var ot = ReadOrderTypeOrEmpty("OrderType (Car/Motorcycle/Bicycle/Walking, empty=keep): ");
            var start = ReadDateTimeOrEmpty("StartOrder (empty=keep): ");
            var dist = ReadDoubleOrEmpty("ActualDistance (empty=keep): ");

            var typeEnd = ReadPossiblyEmpty("TypeEndOrder (empty=keep, '-'=null, else Car/Motorcycle/Bicycle/Walking): ");
            Enum? newTypeEnd = typeEnd switch
            {
                "" => existing.TypeEndOrder,
                "-" => null,
                _ => Enum.TryParse<DO.OrderType>(typeEnd, true, out var v) ? (Enum)v : throw new FormatException("Invalid TypeEndOrder.")
            };

            var timeEnd = ReadPossiblyEmpty("TimeEndOrder (empty=keep, '-'=null, else DateTime): ");
            DateTime? newTimeEnd = timeEnd switch
            {
                "" => existing.TimeEndOrder,
                "-" => null,
                _ => DateTime.TryParse(timeEnd, out var dt) ? dt : throw new FormatException("Invalid DateTime.")
            };

            var updated = existing with
            {
                OrderId = orderId ?? existing.OrderId,
                CourierId = courierId ?? existing.CourierId,
                OrderType = ot.HasValue ? (Enum)ot.Value : existing.OrderType,
                StartOrder = start ?? existing.StartOrder,
                ActualDistance = dist.HasValue ? dist : existing.ActualDistance,
                TypeEndOrder = newTypeEnd,
                TimeEndOrder = newTimeEnd
            };

            s_dalDelivery.Update(updated);
            Console.WriteLine("Delivery updated.");
        }
        catch (Exception ex) { Console.WriteLine(ex); }
    }

    private static void deleteDelivery()
    {
        try
        {
            Console.WriteLine("=== Delete Delivery ===");
            int id = ReadInt("Id: ");
            s_dalDelivery!.Delete(id);
            Console.WriteLine("Delivery deleted.");
        }
        catch (Exception ex) { Console.WriteLine(ex); }
    }

    private static void deleteAllDeliveries()
    {
        try
        {
            Console.WriteLine("=== Delete All Deliveries ===");
            s_dalDelivery!.DeleteAll();
            Console.WriteLine("All Deliveries deleted.");
        }
        catch (Exception ex) { Console.WriteLine(ex); }
    }

    private static void showAllData()
    {
        try
        {
            Console.WriteLine("=== All Couriers ===");
            foreach (var c in s_dalCourier!.ReadAll())
                Console.WriteLine(c);

            Console.WriteLine("=== All Orders ===");
            foreach (var o in s_dalOrder!.ReadAll())
                Console.WriteLine(o);

            Console.WriteLine("=== All Deliveries ===");
            foreach (var d in s_dalDelivery!.ReadAll())
                Console.WriteLine(d);
        }
        catch (Exception ex) { Console.WriteLine(ex); }
    }

    private static void showCONFIG()
    {
        Console.WriteLine("Config Menu:");
        Console.WriteLine("1. Add Minute");
        Console.WriteLine("2. Add Hour");
        Console.WriteLine("3. Add Day");
        Console.WriteLine("4. Add Week");
        Console.WriteLine("5. Add Month");
        Console.WriteLine("6. Add Year");
        Console.WriteLine("7. Show Current Clock");
        Console.WriteLine("8. Update Variable");
        Console.WriteLine("9. Show Variable");
        Console.WriteLine("10. Reset All Config");
        Console.WriteLine("0. EXIT");
    }

    private static void updateVariable()
    {
        Console.WriteLine("Which variable to update?");
        Console.WriteLine("1. Clock (DateTime)");
        Console.WriteLine("2. InactivityRange (TimeSpan)");
        Console.WriteLine("3. ManagerId (int)");
        Console.WriteLine("4. ManagerPassword (string)");
        Console.WriteLine("5. CompanyAddress (string)");
        Console.WriteLine("6. Latitude (double)");
        Console.WriteLine("7. Longitude (double)");
        Console.WriteLine("8. MaxDistanceFromBase (double)");
        Console.WriteLine("9. SpeedInCar (double)");
        Console.WriteLine("10. SpeedInMotorcycle (double)");
        Console.WriteLine("11. SpeedInBicycle (double)");
        Console.WriteLine("12. SpeedInWalking (double)");
        Console.WriteLine("13. MaxTimeSpanForArrival (TimeSpan)");
        Console.WriteLine("14. RiskRange (TimeSpan)");
        Console.Write("Choice: ");

        if (!int.TryParse(Console.ReadLine(), out int ch)) throw new FormatException("Invalid choice.");
        switch (ch)
        {
            case 1: s_dalConfig!.Clock = ReadDateTime("New Clock: "); break;
            case 2: s_dalConfig!.InactivityRange = ReadTimeSpan("New InactivityRange (hh:mm:ss): "); break;
            case 3: s_dalConfig!.ManagerId = ReadInt("New ManagerId: "); break;
            case 4: s_dalConfig!.ManagerPassword = ReadNonEmpty("New ManagerPassword: "); break;
            case 5: s_dalConfig!.CompanyAddress = ReadNonEmpty("New CompanyAddress: "); break;
            case 6: s_dalConfig!.Latitude = ReadDouble("New Latitude: "); break;
            case 7: s_dalConfig!.Longitude = ReadDouble("New Longitude: "); break;
            case 8: s_dalConfig!.MaxDistanceFromBase = ReadDouble("New MaxDistanceFromBase: "); break;
            case 9: s_dalConfig!.SpeedInCar = ReadDouble("New SpeedInCar: "); break;
            case 10: s_dalConfig!.SpeedInMotorcycle = ReadDouble("New SpeedInMotorcycle: "); break;
            case 11: s_dalConfig!.SpeedInBicycle = ReadDouble("New SpeedInBicycle: "); break;
            case 12: s_dalConfig!.SpeedInWalking = ReadDouble("New SpeedInWalking: "); break;
            case 13: s_dalConfig!.MaxTimeSpanForArrival = ReadTimeSpan("New MaxTimeSpanForArrival (hh:mm:ss): "); break;
            case 14: s_dalConfig!.RiskRange = ReadTimeSpan("New RiskRange (hh:mm:ss): "); break;
            default: throw new FormatException("Invalid choice.");
        }
        Console.WriteLine("Config updated.");
    }

    private static void showVariable()
    {
        Console.WriteLine("Which variable to show?");
        Console.WriteLine("1. Clock");
        Console.WriteLine("2. InactivityRange (TimeSpan)");
        Console.WriteLine("3. ManagerId");
        Console.WriteLine("4. ManagerPassword");
        Console.WriteLine("5. CompanyAddress");
        Console.WriteLine("6. Latitude");
        Console.WriteLine("7. Longitude");
        Console.WriteLine("8. MaxDistanceFromBase");
        Console.WriteLine("9. SpeedInCar");
        Console.WriteLine("10. SpeedInMotorcycle");
        Console.WriteLine("11. SpeedInBicycle");
        Console.WriteLine("12. SpeedInWalking");
        Console.WriteLine("13. MaxTimeSpanForArrival (TimeSpan)");
        Console.WriteLine("14. RiskRange (TimeSpan)");
        Console.Write("Choice: ");

        if (!int.TryParse(Console.ReadLine(), out int ch)) throw new FormatException("Invalid choice.");
        switch (ch)
        {
            case 1: Console.WriteLine(s_dalConfig!.Clock); break;
            case 2: Console.WriteLine(s_dalConfig!.InactivityRange); break;
            case 3: Console.WriteLine(s_dalConfig!.ManagerId); break;
            case 4: Console.WriteLine(s_dalConfig!.ManagerPassword); break;
            case 5: Console.WriteLine(s_dalConfig!.CompanyAddress); break;
            case 6: Console.WriteLine(s_dalConfig!.Latitude); break;
            case 7: Console.WriteLine(s_dalConfig!.Longitude); break;
            case 8: Console.WriteLine(s_dalConfig!.MaxDistanceFromBase); break;
            case 9: Console.WriteLine(s_dalConfig!.SpeedInCar); break;
            case 10: Console.WriteLine(s_dalConfig!.SpeedInMotorcycle); break;
            case 11: Console.WriteLine(s_dalConfig!.SpeedInBicycle); break;
            case 12: Console.WriteLine(s_dalConfig!.SpeedInWalking); break;
            case 13: Console.WriteLine(s_dalConfig!.MaxTimeSpanForArrival); break;
            case 14: Console.WriteLine(s_dalConfig!.RiskRange); break;
            default: throw new FormatException("Invalid choice.");
        }
    }

    private static void resetConfigAndData()
    {
        try
        {
            s_dalOrder!.DeleteAll();
            s_dalCourier!.DeleteAll();
            s_dalDelivery!.DeleteAll();
            s_dalConfig!.Reset();
            Console.WriteLine("All data & config have been reset.");
        }
        catch (Exception ex) { Console.WriteLine(ex); }
    }

}



