using System.Diagnostics;
using System.Threading.Tasks;

namespace LotusCore
{
    public static class Logging
    {
        private static readonly SemaphoreSlim _Lock = new SemaphoreSlim(1, 1);

        public static void LogInfo(string msg)
        {
            _Lock.Wait();
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("[INFO] " + msg);
            Console.ForegroundColor = ConsoleColor.White;
            _Lock.Release();
        }

        public static void LogDebug(string msg)
        {
            _Lock.Wait();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("\n[DEBUG] " + msg);
            Console.ForegroundColor = ConsoleColor.White;
            _Lock.Release();
        }

        public static void LogError(string msg, bool showStackTrace = false)
        {
            _Lock.Wait();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[ERROR] " + msg);
            File.AppendAllText("ERROR_Log.txt", $"[ERROR] {msg}\n");
            if (showStackTrace)
            {
                Console.WriteLine(new StackTrace(true));
            }
            Console.ForegroundColor = ConsoleColor.White;
            _Lock.Release();
        }
    }
}
