namespace Dal;
using DalApi;
using DO;
using System.Collections.Generic;

/// <summary>
/// a class that implements the IDelivery interface to manage Delivery entities in the data source.
/// </summary>
internal class DeliveryImplementation : IDelivery
{
    /// <summary>
    /// creates a new delivery and adds it to the data source.
    /// </summary>
    /// <param name="Item"></param>
    public void Create(Delivery Item)
    {
        int IdEntity = Config.NextDeliveryId;
        Delivery copy = Item with { Id = IdEntity };
        DataSource.Deliveries.Add(copy);
    }

    /// <summary>
    /// deletes a delivery from the data source based on its identifier.
    /// </summary>
    /// <param name="IdEntity"></param>
    /// <exception cref="Exception"></exception>
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

    /// <summary>
    /// deletes all deliveries from the data source.
    /// </summary>
    public void DeleteAll()
    {
        DataSource.Deliveries.Clear();
    }

    /// <summary>
    /// retrieves a delivery from the data source based on its identifier.
    /// </summary>
    /// <param name="IdEntity"></param>
    /// <returns></returns>
    public Delivery? Read(int IdEntity)
    {
        return DataSource.Deliveries.Find(c => c.Id == IdEntity);
    }

    /// <summary>
    /// retrieves all deliveries from the data source.
    /// </summary>
    /// <returns></returns>
    public List<Delivery> ReadAll()
    {
        List<Delivery> CopyList = new List<Delivery>();
        foreach (Delivery c in DataSource.Deliveries)
        {
            Delivery NewDelivery = c with { }; // create a copy of the delivery
            CopyList.Add(NewDelivery);
        }
        return CopyList;
    }

    /// <summary>
    /// updates an existing delivery in the data source.
    /// </summary>
    /// <param name="Item"></param>
    /// <exception cref="Exception"></exception>
    public void Update(Delivery Item)
    {
        if (Read(Item.Id) is null)
            throw new Exception($"Delivery with Id {Item.Id} doesn't exist.");
        else
        {
            int Index = DataSource.Deliveries.FindIndex(c => c.Id == Item.Id); // find the index of the delivery to update
            DataSource.Deliveries.RemoveAt(Index);
            DataSource.Deliveries.Add(Item);
        }
    }
}