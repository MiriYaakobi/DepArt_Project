namespace DalApi;

/// <summary>
/// a generic interface for CRUD operations on entity objects in the data access layer.
/// </summary>
/// <typeparam name="T"></typeparam>

public interface ICrud<T> where T : class
{
    void Create(T item); // Creates new entity object in DAL
    T? Read(int id);  // Reads entity object by its ID
    T? Read(Func<T, bool> filter); // Reads entity object by a filter
    IEnumerable<T> ReadAll(Func<T, bool>? filter = null); // Reads all entity objects
    void Update(T item); // Updates entity object
    void Delete(int id); // Deletes an object by its Id
    void DeleteAll(); // Deletes all entity objects
}
