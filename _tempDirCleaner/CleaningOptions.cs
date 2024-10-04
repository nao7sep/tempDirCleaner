namespace _tempDirCleaner
{
    [Flags]
    public enum CleaningOptions
    {
        DeleteAllFiles = 1,
        DeleteFilesOlderThan24Hours = 1 << 1,
        DeleteEmptyDirectories = 1 << 2,
    }
}
