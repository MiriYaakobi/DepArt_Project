namespace Dal;
using DalApi;
using DO;
using System.Collections.Generic;

/// <summary>
/// Provides methods to manage orders, including creating, reading, updating, and deleting orders.
/// </summary>
/// <remarks>This class implements the <see cref="IOrder"/> interface and operates on a data source containing
/// orders. It ensures that each order has a unique identifier and provides functionality to manipulate the order
/// data.</remarks>
public class OrderImplementation : IOrder
{
    /// <summary>
    /// creates a new order and adds it to the data source.
    /// </summary>
    /// <param name="Item"></param>
    public void Create(Order Item)
    {
        int IdEntity = Config.NextOrderId;
        Order Copy = Item with { Id = IdEntity };
        DataSource.Orders.Add(Copy);
    }

    /// <summary>
    /// deletes an order from the data source based on its identifier.
    /// </summary>
    /// <param name="IdEntity"></param>
    /// <exception cref="Exception"></exception>
    public void Delete(int IdEntity)
    {
        if (Read(IdEntity) is null)
            throw new Exception($"Order with Id {IdEntity} doesn't exist.");
        else
        {
            int Index = DataSource.Orders.FindIndex(c => c.Id == IdEntity);
            DataSource.Orders.RemoveAt(Index);
        }
    }

    /// <summary>
    /// deletes all orders from the data source.
    /// </summary>
    public void DeleteAll()
    {
        DataSource.Orders.Clear();
    }

    /// <summary>
    /// retrieves an order from the data source based on its identifier.
    /// </summary>
    /// <param name="IdEntity"></param>
    /// <returns></returns>
    public Order? Read(int IdEntity)
    {
        return DataSource.Orders.Find(c => c.Id == IdEntity); // find the order by its Id
    }

    /// <summary>
    /// retrieves all orders from the data source.
    /// </summary>
    /// <returns></returns>
    public List<Order> ReadAll()
    {
        List<Order> CopyList = new List<Order>();
        foreach (Order c in DataSource.Orders)
        {
            Order NewOrder = c with { }; // create a copy of the order
            CopyList.Add(NewOrder);
        }
        return CopyList;
    }

    /// <summary>
    /// updates an existing order in the data source.
    /// </summary>
    /// <param name="Item"></param>
    /// <exception cref="Exception"></exception>
    public void Update(Order Item)
    {
        if (Read(Item.Id) is null)
            throw new Exception($"Order with Id {Item.Id} doesn't exist.");
        else
        {
            int Index = DataSource.Orders.FindIndex(c => c.Id == Item.Id); // find the index of the order to update
            DataSource.Orders.RemoveAt(Index);
            DataSource.Orders.Add(Item);
        }
    }
}
