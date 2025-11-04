namespace Dal;
using DalApi;
using DO;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class CourierImplementation : ICourier
{
    public void Create(Courier item)
    {
        if (Read(item.Id) is not null)
            throw new Exception($"Courier with Id {item.Id} already exists.");
        DataSource.Couriers.Add(item);
    }

    public void Delete(int id)
    {
        if (Read(id) is null)
            throw new Exception($"Courier with Id {id} doesn't exist.");
        else
        {
            int index = DataSource.Couriers.FindIndex(c => c.Id == id);
            DataSource.Couriers.RemoveAt(index);
        }
    }

    public void DeleteAll()
    {
        DataSource.Couriers.Clear();
    }

    public Courier? Read(int id)
    {
        return DataSource.Couriers.Find(c => c.Id == id);
    }

    public List<Courier> ReadAll()
    {
        List<Courier> CopyList = new List<Courier>();
        foreach (Courier c in DataSource.Couriers)
        {
            Courier newCourier = c with { };
            CopyList.Add(newCourier);
        }
        return CopyList;
    }

    public void Update(Courier item)
    {
        if (Read(item.Id) is null)
            throw new Exception($"Courier with Id {item.Id} doesn't exist.");
        else
        {
            int index = DataSource.Couriers.FindIndex(c => c.Id == item.Id);
            DataSource.Couriers.RemoveAt(index);
            DataSource.Couriers.Add(item);
        }
    }
}
