namespace Dal;
using DalApi;
using DO;
using System.Collections.Generic;

public class DeliveryImplementation : IDelivery
{
    public void Create(Delivery Item)
    {
        int IdEntity = Config.NextDeliveryId;
        Delivery copy = Item with { Id = IdEntity };
        DataSource.Deliveries.Add(copy);
    }

    public void Delete(int IdEntity)
    {
        if (Read(IdEntity) is null)
            throw new Exception($"Delivery with Id {IdEntity} doesn't exist.");
        else
        {
            int Index = DataSource.Deliveries.FindIndex(c => c.Id == IdEntity);
            DataSource.Deliveries.RemoveAt(Index);
        }
    }

    public void DeleteAll()
    {
        DataSource.Deliveries.Clear();
    }

    public Delivery? Read(int IdEntity)
    {
        return DataSource.Deliveries.Find(c => c.Id == IdEntity);
    }

    public List<Delivery> ReadAll()
    {
        List<Delivery> CopyList = new List<Delivery>();
        foreach (Delivery c in DataSource.Deliveries)
        {
            Delivery NewDelivery = c with { };
            CopyList.Add(NewDelivery);
        }
        return CopyList;
    }

    public void Update(Delivery Item)
    {
        if (Read(Item.Id) is null)
            throw new Exception($"Delivery with Id {Item.Id} doesn't exist.");
        else
        {
            int Index = DataSource.Deliveries.FindIndex(c => c.Id == Item.Id);
            DataSource.Deliveries.RemoveAt(Index);
            DataSource.Deliveries.Add(Item);
        }
    }
}