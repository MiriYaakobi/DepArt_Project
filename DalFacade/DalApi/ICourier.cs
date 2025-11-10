namespace DalApi;
using DO;

/// <summary>
/// Defines the operations for managing courier entities in the data access layer.
/// </summary>
/// <remarks>This interface provides methods to create, read, update, and delete courier entities. Implementations
/// of this interface should handle the persistence and retrieval of courier data.</remarks>
public interface ICourier : ICrud<Courier> { }
