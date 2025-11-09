namespace DalApi;
using DO;

/// <summary>
/// Defines the operations for managing courier entities in the data access layer.
/// </summary>
/// <remarks>This interface provides methods to create, read, update, and delete courier entities. Implementations
/// of this interface should handle the persistence and retrieval of courier data.</remarks>
public interface ICourier
{
    void Create(Courier item); //Creates new entity object in DAL
    Courier? Read(int id); //Reads entity object by its ID 
    List<Courier> ReadAll(); //Reads all entity objects
    void Update(Courier item); //Updates entity object
    void Delete(int id); //Deletes an object by its Id
    void DeleteAll(); //Delete all entity objects

}