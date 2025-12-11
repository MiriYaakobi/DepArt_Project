namespace BLApi;
public static class Factory
{
    public static IBl Get() => new BlImplementation.Bl();
}
