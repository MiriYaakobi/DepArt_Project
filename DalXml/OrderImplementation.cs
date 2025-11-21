namespace Dal;
using DalApi;
using DO;
using System;
using System.Collections.Generic;

/// <summary>
/// Order implementation of the data access layer
/// </summary>
internal class OrderImplementation : IOrder
{
    /// <summary>
    /// create a new order
    /// </summary>
    /// <param name="item"></param>
    public void Create(Order item)
    {
        List<Order> orders = XMLTools.LoadListFromXMLSerializer<Order>(Config.s_orders_xml);

        // check if the order already exists
        if (orders.Exists(o => o.Id == item.Id))
            throw new DalAlreadyExistsException($"Order with ID={item.Id} already exists");

        orders.Add(item);
        XMLTools.SaveListToXMLSerializer(orders, Config.s_orders_xml);
    }

    /// <summary>
    /// delete an order by id
    /// </summary>
    /// <param name="id"></param>
    /// <exception cref="DalDoesNotExistException"></exception>
    public void Delete(int id)
    {
        List<Order> orders = XMLTools.LoadListFromXMLSerializer<Order>(Config.s_orders_xml);

        // remove the order
        if (orders.RemoveAll(it => it.Id == id) == 0)
            throw new DalDoesNotExistException($"Order with ID={id} does Not exist");

        XMLTools.SaveListToXMLSerializer(orders, Config.s_orders_xml);

    }

    /// <summary>
    /// delete all orders
    /// </summary>
    public void DeleteAll()
    {
        // save an empty list to the xml file
        XMLTools.SaveListToXMLSerializer(new List<Order>(), Config.s_orders_xml);
    }

    /// <summary>
    /// read an order by id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public Order? Read(int id)
    {
        List<Order> orders = XMLTools.LoadListFromXMLSerializer<Order>(Config.s_orders_xml);
        return orders.FirstOrDefault(it => it.Id == id);
    }

    /// <summary>
    /// read an order that match the filter
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    public Order? Read(Func<Order, bool> filter)
    {
        List<Order> orders = XMLTools.LoadListFromXMLSerializer<Order>(Config.s_orders_xml);
        return orders.FirstOrDefault(filter);
    }

    /// <summary>
    /// read all orders that match the filter
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    public IEnumerable<Order> ReadAll(Func<Order, bool>? filter = null)
    {
        List<Order> orders = XMLTools.LoadListFromXMLSerializer<Order>(Config.s_orders_xml);

        // if there is a filter, apply it
        if (filter != null)
            return orders.Where(filter);

        return orders;
    }

    /// <summary>
    /// update an order
    /// </summary>
    /// <param name="item"></param>
    /// <exception cref="DalDoesNotExistException"></exception>
    public void Update(Order item)
    {
        List<Order> orders = XMLTools.LoadListFromXMLSerializer<Order>(Config.s_orders_xml);

        // remove the old order
        if (orders.RemoveAll(it => it.Id == item.Id) == 0)
            throw new DalDoesNotExistException($"Order with ID={item.Id} does Not exist");

        orders.Add(item);
        XMLTools.SaveListToXMLSerializer(orders, Config.s_orders_xml);
    }
}