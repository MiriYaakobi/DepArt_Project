namespace DalApi;
using DO;

/// <summary>
/// Defines the contract for managing delivery entities, including operations to create, read, update, and delete
/// deliveries.
/// </summary>
/// <remarks>This interface provides methods to perform CRUD operations on delivery entities. Implementations
/// should ensure that operations are performed in a consistent and reliable manner, handling any necessary data
/// persistence or retrieval logic.</remarks>
public interface IDelivery : ICrud<Delivery> { }
