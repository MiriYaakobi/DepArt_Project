namespace Dal;
using DalApi;
using DO;
using System.Collections.Generic;

public class OrderImplementation : IOrder
{
    public void Create(Order item)
    {
        int id = Config.NextOrderId;
        Order copy = item with { Id = id };
        DataSource.Orders.Add(copy);
    }

    public void Delete(int id)
    {
        if (Read(id) is null)
            throw new Exception($"Order with Id {id} doesn't exist.");
        else
        {
            int index = DataSource.Orders.FindIndex(c => c.Id == id);
            DataSource.Orders.RemoveAt(index);
        }
    }

    public void DeleteAll()
    {
        DataSource.Orders.Clear();
    }

    public Order? Read(int id)
    {
        return DataSource.Orders.Find(c => c.Id == id);
    }

    public List<Order> ReadAll()
    {
        List<Order> CopyList = new List<Order>();
        foreach (Order c in DataSource.Orders)
        {
            Order newOrder = c with { };
            CopyList.Add(newOrder);
        }
        return CopyList;
    }

    public void Update(Order item)
    {
        if (Read(item.Id) is null)
            throw new Exception($"Order with Id {item.Id} doesn't exist.");
        else
        {
            int index = DataSource.Orders.FindIndex(c => c.Id == item.Id);
            DataSource.Orders.RemoveAt(index);
            DataSource.Orders.Add(item);
        }
    }
}
