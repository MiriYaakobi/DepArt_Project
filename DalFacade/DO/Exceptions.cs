namespace DO;

/// <summary>
/// exception thrown when a requested entity does not exist in the data source.
/// </summary>
[Serializable]
public class DalDoesNotExistException : Exception
{
    public DalDoesNotExistException(string? message) : base(message) { }
}

/// <summary>
/// exception thrown when attempting to create an entity that already exists in the data source.
/// </summary>
[Serializable]
public class DalAlreadyExistsException : Exception
{
    public DalAlreadyExistsException(string? message) : base(message) { }
}

/// <summary>
/// exception thrown when a null value is encountered where it is not allowed.
/// </summary>
[Serializable]
public class DalNullValueException : Exception
{
    public DalNullValueException(string? message) : base(message) { }
}

/// <summary>
/// exception thrown when there is an error loading or creating the XML file.
/// </summary>
[Serializable]
public class DalXMLFileLoadCreateException : Exception
{
    public DalXMLFileLoadCreateException(string? message) : base(message) { }
}