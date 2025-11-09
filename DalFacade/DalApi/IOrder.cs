namespace DalApi;
using DO;

/// <summary>
/// Defines the contract for managing order entities, including operations to create, read, update, and delete orders.
/// </summary>
/// <remarks>This interface provides methods for basic CRUD operations on order entities. Implementations should
/// ensure that operations are performed in a consistent and reliable manner, handling any necessary data validation and
/// error management.</remarks>
public interface IOrder : ICrud<Order> { }
