public static class Logging
{
    public static void LogInfo(string msg)
    {
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine(msg);
        Console.ForegroundColor = ConsoleColor.White;
    }
    public static void LogDebug(string msg)
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine(msg);
        Console.ForegroundColor = ConsoleColor.White;
    }
    public static void LogError(string msg)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(msg);
        Console.ForegroundColor = ConsoleColor.White;
    }
}