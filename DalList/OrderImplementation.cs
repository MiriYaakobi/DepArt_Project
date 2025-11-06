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
    public void Create(Order Item)
    {
        int IdEntity = Config.NextOrderId;
        Order Copy = Item with { Id = IdEntity };
        DataSource.Orders.Add(Copy);
    }

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

    public void DeleteAll()
    {
        DataSource.Orders.Clear();
    }

    public Order? Read(int IdEntity)
    {
        return DataSource.Orders.Find(c => c.Id == IdEntity);
    }

    public List<Order> ReadAll()
    {
        List<Order> CopyList = new List<Order>();
        foreach (Order c in DataSource.Orders)
        {
            Order NewOrder = c with { };
            CopyList.Add(NewOrder);
        }
        return CopyList;
    }

    public void Update(Order Item)
    {
        if (Read(Item.Id) is null)
            throw new Exception($"Order with Id {Item.Id} doesn't exist.");
        else
        {
            int Index = DataSource.Orders.FindIndex(c => c.Id == Item.Id);
            DataSource.Orders.RemoveAt(Index);
            DataSource.Orders.Add(Item);
        }
    }
}
