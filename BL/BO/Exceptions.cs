namespace BO;

/// <summary>
/// an abstract class representing a business logic exception
/// </summary>
[Serializable]
public abstract class BlException : Exception
{
    protected BlException(string message) : base(message) { }
    protected BlException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// a business logic exception representing an entity that does not exist
/// </summary>
[Serializable]
public class BlDoesNotExistException : BlException
{
    public BlDoesNotExistException(string message) : base(message) { }
    public BlDoesNotExistException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// a business logic exception representing an entity that already exists
/// </summary>
[Serializable]
public class BlAlreadyExistsException : BlException
{
    public BlAlreadyExistsException(string message) : base(message) { }
    public BlAlreadyExistsException(string message, Exception innerException) : base(message, innerException) { }
}

//---BL exceptions for authorization and authentication issues---

/// <summary>
/// a business logic exception representing a not authorized action
/// </summary>
[Serializable]
public class BlNotAuthorizedException : BlException
{
    public BlNotAuthorizedException(string message) : base(message) { }
}

/// <summary>
/// a business logic exception representing a login failure
/// </summary>
[Serializable]
public class BlLoginFailedException : BlException
{
    public BlLoginFailedException(string message) : base(message) { }
}

/// <summary>
/// a business logic exception representing invalid data
/// </summary>
[Serializable]
public class BlInvalidDataException : BlException
{
    public BlInvalidDataException(string message) : base(message) { }
    public BlInvalidDataException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// a business logic exception representing a temporary unavailability
/// </summary>
[Serializable]
public class BlTemporaryNotAvailableException : BlException
{
    public BlTemporaryNotAvailableException(string message) : base(message) { }
}

/// <summary>
/// a business logic exception representing an inability to delete an entity
/// </summary>
[Serializable]
public class BlCannotDeleteException : BlException
{
    public BlCannotDeleteException(string message) : base(message) { }
}

/// <summary>
/// a business logic exception representing an invalid operation
/// </summary>
[Serializable]
public class BlInvalidOperationException : BlException
{
    public BlInvalidOperationException(string message) : base(message) { }
    public BlInvalidOperationException(string message, Exception innerException) : base(message, innerException) { }
}

