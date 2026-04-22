using Serilog;

namespace ECommerce.SharedLibrary.Logs
{
    public static class LogException
    {
        public static void LogExceptions(Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");

            LogToFile(ex.Message);
            LogToConsole(ex.Message);
            LogToDebug(ex.Message);

        }

        public static void LogToDebug(string message)
        {
            Log.Information(message);
        }
        public static void LogToConsole(string message)
        {
            Log.Warning(message);
        }
        public static void LogToFile(string message)
        {
            Log.Debug(message);
        }
    }
}
