using System.Diagnostics;
using System.Threading.Tasks;

namespace Core_Engine
{
    public static class Logging
    {
        private static readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        public static void LogInfo(string msg)
        {
            _lock.Wait();
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("[INFO] " + msg);
            Console.ForegroundColor = ConsoleColor.White;
            _lock.Release();
        }

        public static void LogDebug(string msg)
        {
            _lock.Wait();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("\n[DEBUG] " + msg);
            Console.ForegroundColor = ConsoleColor.White;
            _lock.Release();
        }

        public static void LogError(string msg, bool showStackTrace = false)
        {
            _lock.Wait();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[ERROR] " + msg);
            if (showStackTrace)
            {
                Console.WriteLine(new StackTrace(true));
            }
            Console.ForegroundColor = ConsoleColor.White;
            _lock.Release();
        }
    }
}
