namespace Web.Client.Blazor.Utilities.Common;

public static class IdGenerator
{
    private static int _currentId = 0;

    public static int GetElevatorNextId()
    {
        return Interlocked.Increment(ref _currentId);
    }

    public static int GetRequestNextId()
    {
        return Interlocked.Increment(ref _currentId);
    }
}
