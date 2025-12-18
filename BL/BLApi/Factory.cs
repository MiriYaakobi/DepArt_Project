namespace BLApi;

/// <summary>
/// a factory class for creating instances of the business logic layer
/// </summary>
/// <remarks>This factory class provides a method to obtain an instance of the business logic layer (BL).
public static class Factory
{
    public static IBl Get() => new BlImplementation.Bl();
}
