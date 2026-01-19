namespace Dal;
using DalApi;
using DO;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

/// <summary>
/// Provides methods to manage orders, including creating, reading, updating, and deleting orders.
/// </summary>
/// <remarks>This class implements the <see cref="IOrder"/> interface and operates on a data source containing
/// orders. It ensures that each order has a unique identifier and provides functionality to manipulate the order
/// data.</remarks>
internal class OrderImplementation : IOrder
{
    /// <summary>
    /// creates a new order and adds it to the data source.
    /// </summary>
    /// <param name="Item"></param>
    [MethodImpl(MethodImplOptions.Synchronized)]
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
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Delete(int IdEntity)
    {
        if (Read(IdEntity) is null)
            throw new DalDoesNotExistException($"Order with Id {IdEntity} doesn't exist.");
        else
        {
            int Index = DataSource.Orders.FindIndex(c => c.Id == IdEntity);
            DataSource.Orders.RemoveAt(Index);
        }
    }

    /// <summary>
    /// deletes all orders from the data source.
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void DeleteAll()
    {
        DataSource.Orders.Clear();
    }

    /// <summary>
    /// retrieves an order from the data source based on its identifier.
    /// </summary>
    /// <param name="IdEntity"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public Order? Read(int IdEntity)
    {
        return DataSource.Orders.FirstOrDefault(c => c.Id == IdEntity); // find the order by its Id
    }

    /// <summary>
    /// retrieves an order from the data source based on a filter.
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public DO.Order? Read(Func<DO.Order, bool> filter)
        => DataSource.Orders.FirstOrDefault(filter);

    /// <summary>
    /// retrieves all orders from the data source.
    /// </summary>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public IEnumerable<Order> ReadAll(Func<Order, bool>? filter = null)
        => filter == null
            ? DataSource.Orders.Select(c => c)
            : DataSource.Orders.Where(filter);

    /// <summary>
    /// updates an existing order in the data source.
    /// </summary>
    /// <param name="Item"></param>
    /// <exception cref="Exception"></exception>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Update(Order Item)
    {
        if (Read(Item.Id) is null)
            throw new DalDoesNotExistException($"Order with Id {Item.Id} doesn't exist.");
        else
        {
            int Index = DataSource.Orders.FindIndex(c => c.Id == Item.Id); // find the index of the order to update
            DataSource.Orders.RemoveAt(Index);
            DataSource.Orders.Add(Item);
        }
    }
}
