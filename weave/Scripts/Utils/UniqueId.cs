namespace weave.Utils;

public static class UniqueId
{
    private static int _counter = 1;

    public static string Generate()
    {
        return _counter++.ToString();
    }
}
