namespace Dal;
using DalApi;
using DO;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

/// <summary>
/// courier data access implementation using XElement XML storage
/// </summary>
internal class CourierImplementation : ICourier
{
    /// <summary>
    /// creates a new courier
    /// </summary>
    /// <param name="item"></param>
    /// <exception cref="DalAlreadyExistsException"></exception>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Create(Courier item)
    {
        // load the couriers XML
        XElement couriersRoot = XMLTools.LoadListFromXMLElement(Config.s_couriers_xml);

        // check if a courier with the same id already exists
        if (couriersRoot.Elements("Courier").Any(c => c.ToIntNullable("Id") == item.Id))
            throw new DalAlreadyExistsException($"courier with id {item.Id} already exists");

        //create the courier element and add it to the XML
        couriersRoot.Add(CreateCourierElement(item));

        // save the updated XML
        XMLTools.SaveListToXMLElement(couriersRoot, Config.s_couriers_xml);
    }

    /// <summary>
    /// deletes the courier with the given id
    /// </summary>
    /// <param name="id"></param>
    /// <exception cref="DalDoesNotExistException"></exception>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Delete(int id)
    {
        XElement couriersRoot = XMLTools.LoadListFromXMLElement(Config.s_couriers_xml);
        XElement? courierToDelete = couriersRoot.Elements("Courier")
            .FirstOrDefault(c => c.ToIntNullable("Id") == id);

        if (courierToDelete is null)
            throw new DalDoesNotExistException($"courier with id {id} does not exist");
     
        courierToDelete.Remove();
        XMLTools.SaveListToXMLElement(couriersRoot, Config.s_couriers_xml);
    }

    /// <summary>
    /// deletes all couriers
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void DeleteAll()
    {
        XElement couriersRoot = XMLTools.LoadListFromXMLElement(Config.s_couriers_xml);
        couriersRoot.RemoveAll();
        XMLTools.SaveListToXMLElement(couriersRoot, Config.s_couriers_xml);
    }

    /// <summary>
    /// returns the courier with the given id, or null if not found
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public Courier? Read(int id)
    {
        XElement couriersRoot = XMLTools.LoadListFromXMLElement(Config.s_couriers_xml);
        XElement? courierElement = couriersRoot.Elements("Courier")
            .FirstOrDefault(c => c.ToIntNullable("Id") == id);

        return ConvertXElementToCourier(courierElement);
    }

    /// <summary>
    /// returns the first courier that matches the filter, or null if none found
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public Courier? Read(Func<Courier, bool> filter)
    {
        return XMLTools.LoadListFromXMLElement(Config.s_couriers_xml).Elements()
        .Select(c => ConvertXElementToCourier(c))
        .FirstOrDefault(c => c != null && filter(c));
    }

    /// <summary>
    /// returns all couriers that match the filter, or all couriers if the filter is null
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public IEnumerable<Courier> ReadAll(Func<Courier, bool>? filter = null)
    {
        return XMLTools.LoadListFromXMLElement(Config.s_couriers_xml).Elements()
         .Select(c => ConvertXElementToCourier(c))
         .Where(c => c != null && (filter == null || filter(c)))
         .Cast<Courier>();
    }

    /// <summary>
    /// updates the name and phone of the courier with the same id as item.Id
    /// </summary>
    /// <param name="item"></param>
    /// <exception cref="DalDoesNotExistException"></exception>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Update(Courier item)
    {
        XElement couriersRoot = XMLTools.LoadListFromXMLElement(Config.s_couriers_xml);
        XElement? courierToUpdate = couriersRoot.Elements("Courier")
            .FirstOrDefault(c => c.ToIntNullable("Id") == item.Id);

        // check if the courier exists
        if (courierToUpdate is null)
            throw new DalDoesNotExistException($"courier with id {item.Id} does not exist");

        // replace the old courier element with the updated one
        courierToUpdate.ReplaceWith(CreateCourierElement(item));

        XMLTools.SaveListToXMLElement(couriersRoot, Config.s_couriers_xml);
    }

    /// <summary>
    /// helpers convert XElement to Courier
    /// </summary>
    /// <param name="element"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.Synchronized)]
    private Courier? ConvertXElementToCourier(XElement? element)
    {
        if (element == null)
            return null;

        return new Courier(
            Id: element.ToIntNullable("Id") ?? 0,
            Name: element.ToStringNullable("Name") ?? string.Empty,
            Phone: element.ToStringNullable("Phone") ?? string.Empty,
            Email: element.ToStringNullable("Email") ?? string.Empty,
            Password: element.ToStringNullable("Password") ?? string.Empty,
            IsActive: element.ToBoolNullable("IsActive") ?? false,
            TypeOfDelivery: element.ToEnumNullable<DeliveryType>("TypeOfDelivery") ?? DeliveryType.ByFoot,
            StartWorkTime: element.ToDateTimeNullable("StartWorkTime") ?? DateTime.Now,
            MaxDistance: element.ToDoubleNullable("MaxDistance")
        );
    }

    /// <summary>
    /// helpers create XElement from Courier
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.Synchronized)]
    private XElement CreateCourierElement(Courier item)
    {
        return 
            new XElement("Courier",
            new XElement("Id", item.Id),
            new XElement("Name", item.Name),
            new XElement("Phone", item.Phone),
            new XElement("Email", item.Email),
            new XElement("Password", item.Password),
            new XElement("IsActive", item.IsActive),
            new XElement("TypeOfDelivery", item.TypeOfDelivery),
            new XElement("StartWorkTime", item.StartWorkTime),
            new XElement("MaxDistance", item.MaxDistance)
        );
    }
}