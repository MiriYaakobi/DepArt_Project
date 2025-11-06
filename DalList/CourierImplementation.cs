namespace Dal;
using DalApi;
using DO;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class CourierImplementation : ICourier
{
    public void Create(Courier Item)
    {
        if (Read(Item.Id) is not null)
            throw new Exception($"Courier with Id {Item.Id} already exists.");
        DataSource.Couriers.Add(Item);
    }

    public void Delete(int IdEntity)
    {
        if (Read(IdEntity) is null)
            throw new Exception($"Courier with Id {IdEntity} doesn't exist.");
        else
        {
            int Index = DataSource.Couriers.FindIndex(c => c.Id == IdEntity);
            DataSource.Couriers.RemoveAt(Index);
        }
    }

    public void DeleteAll()
    {
        DataSource.Couriers.Clear();
    }

    public Courier? Read(int IdEntity)
    {
        return DataSource.Couriers.Find(c => c.Id == IdEntity);
    }

    public List<Courier> ReadAll()
    {
        List<Courier> CopyList = new List<Courier>();
        foreach (Courier c in DataSource.Couriers)
        {
            Courier NewCourier = c with { };
            CopyList.Add(NewCourier);
        }
        return CopyList;
    }

    public void Update(Courier Item)
    {
        if (Read(Item.Id) is null)
            throw new Exception($"Courier with Id {Item.Id} doesn't exist.");
        else
        {
            int Index = DataSource.Couriers.FindIndex(c => c.Id == Item.Id);
            DataSource.Couriers.RemoveAt(Index);
            DataSource.Couriers.Add(Item);
        }
    }
}