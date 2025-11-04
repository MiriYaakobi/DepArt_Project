namespace Dal;
using DalApi;
using DO;
using System.Collections.Generic;

public class DeliveryImplementation : IDelivery
{
    public void Create(Delivery item)
    {
        int id = Config.NextDeliveryId;
        Delivery copy = item with { Id = id };
        DataSource.Deliveries.Add(copy);
    }

    public void Delete(int id)
    {
        if (Read(id) is null)
            throw new Exception($"Delivery with Id {id} doesn't exist.");
        else
        {
            int index = DataSource.Deliveries.FindIndex(c => c.Id == id);
            DataSource.Deliveries.RemoveAt(index);
        }
    }

    public void DeleteAll()
    {
        DataSource.Deliveries.Clear();
    }

    public Delivery? Read(int id)
    {
        return DataSource.Deliveries.Find(c => c.Id == id);
    }

    public List<Delivery> ReadAll()
    {
        List<Delivery> CopyList = new List<Delivery>();
        foreach (Delivery c in DataSource.Deliveries)
        {
            Delivery newDelivery = c with { };
            CopyList.Add(newDelivery);
        }
        return CopyList;
    }

    public void Update(Delivery item)
    {
        if (Read(item.Id) is null)
            throw new Exception($"Delivery with Id {item.Id} doesn't exist.");
        else
        {
            int index = DataSource.Deliveries.FindIndex(c => c.Id == item.Id);
            DataSource.Deliveries.RemoveAt(index);
            DataSource.Deliveries.Add(item);
        }
    }
}
