namespace DalApi;
using DO;

/// <summary>
/// Defines the contract for managing order entities, including operations to create, read, update, and delete orders.
/// </summary>
/// <remarks>This interface provides methods for basic CRUD operations on order entities. Implementations should
/// ensure that operations are performed in a consistent and reliable manner, handling any necessary data validation and
/// error management.</remarks>
public interface IOrder
{
    void Create(Order item); //Creates new entity object in DAL
    Order? Read(int id); //Reads entity object by its ID 
    List<Order> ReadAll(); //Reads all entity objects
    void Update(Order item); //Updates entity object
    void Delete(int id); //Deletes an object by its Id
    void DeleteAll(); //Delete all entity objects

}