using System.Reflection;

namespace _tempDirCleaner
{
    public static class Utility
    {
        public static string AppDirectoryPath { get; } = Path.GetDirectoryName (Assembly.GetExecutingAssembly ().Location)!;

        public static string TargetsFileName { get; } = "Targets.txt";

        public static string TargetsFilePath { get; } = Path.Join (AppDirectoryPath, TargetsFileName);

        public static string LogsDirectoryPath { get; } = Path.Join (AppDirectoryPath, "Logs");
    }
}
