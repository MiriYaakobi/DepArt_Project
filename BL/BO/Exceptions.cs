namespace BO;

[Serializable]
public abstract class BlException : Exception
{
    protected BlException(string message) : base(message) { }
    protected BlException(string message, Exception innerException) : base(message, innerException) { }
}

[Serializable]
public class BlDoesNotExistException : BlException
{
    public BlDoesNotExistException(string message) : base(message) { }
    public BlDoesNotExistException(string message, Exception innerException) : base(message, innerException) { }
}

[Serializable]
public class BlAlreadyExistsException : BlException
{
    public BlAlreadyExistsException(string message) : base(message) { }
    public BlAlreadyExistsException(string message, Exception innerException) : base(message, innerException) { }
}

//BL exceptions for authorization and authentication issues

[Serializable]
public class BlNotAuthorizedException : BlException
{
    public BlNotAuthorizedException(string message) : base(message) { }
}

[Serializable]
public class BlLoginFailedException : BlException
{
    public BlLoginFailedException(string message) : base(message) { }
}

[Serializable]
public class BlInvalidDataException : BlException
{
    public BlInvalidDataException(string message) : base(message) { }
    public BlInvalidDataException(string message, Exception innerException) : base(message, innerException) { }
}

[Serializable]
public class BlTemporaryNotAvailableException : BlException
{
    public BlTemporaryNotAvailableException(string message) : base(message) { }
}

[Serializable]
public class BlCannotDeleteException : BlException
{
    public BlCannotDeleteException(string message) : base(message) { }
}

[Serializable]
public class BlInvalidOperationException : BlException
{
    public BlInvalidOperationException(string message) : base(message) { }
    public BlInvalidOperationException(string message, Exception innerException) : base(message, innerException) { }
}

