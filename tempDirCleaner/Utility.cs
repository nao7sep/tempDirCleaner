using System.Reflection;

namespace tempDirCleaner
{
    public static class Utility
    {
        public static string AppDirectoryPath { get; } = Path.GetDirectoryName (Assembly.GetExecutingAssembly ().Location)!;

        public static string TargetsFileName { get; } = "Targets.txt";

        public static string TargetsFilePath { get; } = Path.Join (AppDirectoryPath, TargetsFileName);

        public static string LogsDirectoryPath { get; } = Path.Join (AppDirectoryPath, "Logs");

        public static string LogFilePath { get; } = Path.Join (LogsDirectoryPath, $"Log-{DateTime.UtcNow:yyyyMMdd'T'HHmmss'Z'}.log");

        // File.WriteAllLines wont take a list of nullable strings.
        public static List <string> Logs { get; } = [];

        public static void AddLogLine (string? message = null) => Logs.Add (message ?? string.Empty);
    }
}
