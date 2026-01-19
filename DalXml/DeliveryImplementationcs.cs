namespace Dal;
using DalApi;
using DO;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;


/// <summary>
/// Delivery implementation of the data access layer
/// </summary>
internal class DeliveryImplementation : IDelivery
{
    /// <summary>
    /// Create a new delivery
    /// </summary>
    /// <param name="item"></param>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Create(Delivery item)
    {
        int nextId = Config.NextDeliveryId;
        Delivery copy = item with { Id = nextId };

        List<Delivery> deliveries = XMLTools.LoadListFromXMLSerializer<Delivery>(Config.s_deliveries_xml);

        // check if the order already exists
        if (deliveries.Exists(d => d.Id == item.Id))
            throw new DalAlreadyExistsException($"Delivery with ID={item.Id} already exists");

        deliveries.Add(copy);
        XMLTools.SaveListToXMLSerializer(deliveries, Config.s_deliveries_xml);
    }

    /// <summary>
    /// delete a delivery by id
    /// </summary>
    /// <param name="id"></param>
    /// <exception cref="DalDoesNotExistException"></exception>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Delete(int id)
    {
        List<Delivery> deliveries = XMLTools.LoadListFromXMLSerializer<Delivery>(Config.s_deliveries_xml);

        // remove the delivery
        if (deliveries.RemoveAll(it => it.Id == id) == 0)
            throw new DalDoesNotExistException($"Delivery with ID={id} does Not exist");

        XMLTools.SaveListToXMLSerializer(deliveries, Config.s_deliveries_xml);
    }

    /// <summary>
    /// delete all deliveries
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void DeleteAll()
    {
        // save an empty list to the xml file
        XMLTools.SaveListToXMLSerializer(new List<Delivery>(), Config.s_deliveries_xml);
    }

    /// <summary>
    /// read a delivery by id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public Delivery? Read(int id)
    {
        List<Delivery> deliveries = XMLTools.LoadListFromXMLSerializer<Delivery>(Config.s_deliveries_xml);
        return deliveries.FirstOrDefault(it => it.Id == id);
    }

    /// <summary>
    /// read a delivery by a custom filter
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public Delivery? Read(Func<Delivery, bool> filter)
    {
        List<Delivery> deliveries = XMLTools.LoadListFromXMLSerializer<Delivery>(Config.s_deliveries_xml);
        return deliveries.FirstOrDefault(filter);
    }

    /// <summary>
    /// read all deliveries
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public IEnumerable<Delivery> ReadAll(Func<Delivery, bool>? filter = null)
    {
        List<Delivery> deliveries = XMLTools.LoadListFromXMLSerializer<Delivery>(Config.s_deliveries_xml);

        // if there is a filter, apply it
        if (filter != null)
            return deliveries.Where(filter);

        return deliveries;
    }

    /// <summary>
    /// Update an existing delivery
    /// </summary>
    /// <param name="item"></param>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Update(Delivery item)
    {
        List<Delivery> deliveries = XMLTools.LoadListFromXMLSerializer<Delivery>(Config.s_deliveries_xml);

        // remove the old delivery
        if (deliveries.RemoveAll(it => it.Id == item.Id) == 0)
            throw new DalDoesNotExistException($"Delivery with ID={item.Id} does Not exist");

        deliveries.Add(item);
        XMLTools.SaveListToXMLSerializer(deliveries, Config.s_deliveries_xml);
    }
}
