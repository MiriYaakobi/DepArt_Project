namespace Dal;
using DalApi;
using DO;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// a class that implements the ICourier interface to manage Courier entities in the data source.
/// </summary>
internal class CourierImplementation : ICourier
{
    /// <summary>
    /// creates a new courier and adds it to the data source.
    /// </summary>
    /// <param name="Item"></param>
    /// <exception cref="Exception"></exception>
    public void Create(Courier Item)
    {
        if (Read(Item.Id) is not null)
            throw new DalAlreadyExistsException($"Courier with Id {Item.Id} already exists.");
        DataSource.Couriers.Add(Item);
    }

    /// <summary>
    /// deletes a courier from the data source based on its identifier.
    /// </summary>
    /// <param name="IdEntity"></param>
    /// <exception cref="Exception"></exception>
    public void Delete(int IdEntity)
    {
        if (Read(IdEntity) is null)
            throw new DalDoesNotExistException($"Courier with Id {IdEntity} doesn't exist.");
        else
        {
            int Index = DataSource.Couriers.FindIndex(c => c.Id == IdEntity);
            DataSource.Couriers.RemoveAt(Index);
        }
    }

    /// <summary>
    /// deletes all couriers from the data source.
    /// </summary>
    public void DeleteAll()
    {
        DataSource.Couriers.Clear();
    }

    /// <summary>
    /// retrieves a courier from the data source based on its identifier.
    /// </summary>
    /// <param name="IdEntity"></param>
    /// <returns></returns>
    public Courier? Read(int IdEntity)
    {
        return DataSource.Couriers.FirstOrDefault(c => c.Id == IdEntity);
    }


    /// <summary>
    /// retrieves a courier from the data source based on a filter.
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    public DO.Courier? Read(Func<DO.Courier, bool> filter)
        => DataSource.Couriers.FirstOrDefault(filter);

    /// <summary>
    /// retrieves all couriers from the data source.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<Courier> ReadAll(Func<Courier, bool>? filter = null)
        => filter == null
          ? DataSource.Couriers.Select(c => c)
          : DataSource.Couriers.Where(filter);

    /// <summary>
    /// updates an existing courier in the data source.
    /// </summary>
    /// <param name="Item"></param>
    /// <exception cref="Exception"></exception>
    public void Update(Courier Item)
    {
        if (Read(Item.Id) is null)
            throw new DalDoesNotExistException($"Courier with Id {Item.Id} doesn't exist.");
        else
        {
            int Index = DataSource.Couriers.FindIndex(c => c.Id == Item.Id); // find the index of the courier to update
            DataSource.Couriers.RemoveAt(Index);
            DataSource.Couriers.Add(Item);
        }
    }
}