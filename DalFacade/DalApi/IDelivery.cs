namespace DalApi;
using DO;

/// <summary>
/// Defines the contract for managing delivery entities, including operations to create, read, update, and delete
/// deliveries.
/// </summary>
/// <remarks>This interface provides methods to perform CRUD operations on delivery entities. Implementations
/// should ensure that operations are performed in a consistent and reliable manner, handling any necessary data
/// persistence or retrieval logic.</remarks>
public interface IDelivery
{
    void Create(Delivery item); //Creates new entity object in DAL
    Delivery? Read(int id); //Reads entity object by its ID 
    List<Delivery> ReadAll(); //Reads all entity objects
    void Update(Delivery item); //Updates entity object
    void Delete(int id); //Deletes an object by its Id
    void DeleteAll(); //Delete all entity objects
}