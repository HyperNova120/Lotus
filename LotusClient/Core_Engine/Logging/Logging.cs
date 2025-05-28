namespace Core_Engine
{
    public static class Logging
    {
        public static Mutex mut = new();

        public static void LogInfo(string msg)
        {
            mut.WaitOne();
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(msg);
            Console.ForegroundColor = ConsoleColor.White;
            mut.ReleaseMutex();
        }

        public static void LogDebug(string msg)
        {
            mut.WaitOne();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(msg);
            Console.ForegroundColor = ConsoleColor.White;
            mut.ReleaseMutex();
        }

        public static void LogError(string msg)
        {
            mut.WaitOne();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(msg);
            Console.ForegroundColor = ConsoleColor.White;
            mut.ReleaseMutex();
        }
    }
}
