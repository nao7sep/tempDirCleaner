using System.Text;

namespace _tempDirCleaner
{
    class Program
    {
        static void Main (string [] args)
        {
            try
            {
                if (File.Exists (Utility.TargetsFilePath) == false)
                    File.WriteAllText (Utility.TargetsFilePath, string.Empty, Encoding.UTF8);

                List <(string DirectoryPath, CleaningOptions Options)> xTargets = [];

                foreach (string xLine in File.ReadAllLines (Utility.TargetsFilePath, Encoding.UTF8))
                {
                    if (string.IsNullOrWhiteSpace (xLine) || xLine.StartsWith ("//"))
                        continue;

                    string [] xParts = xLine.Split ('|');

                    if (xParts.Length == 2)
                    {
                        string xDirectoryPath = xParts [0].Trim ();

                        if (Directory.Exists (xDirectoryPath) || Path.IsPathFullyQualified (xDirectoryPath)) // Currently nonexistent directories are allowed.
                        {
                            if (Enum.TryParse <CleaningOptions> (xParts [1].Trim (), ignoreCase: true, out var xOptions))
                            {
                                if (xOptions.HasFlag (CleaningOptions.DeleteAllFiles) || xOptions.HasFlag (CleaningOptions.DeleteFilesOlderThan24Hours)) // At least one of these options must be set.
                                {
                                    xTargets.Add ((xDirectoryPath, xOptions));
                                    continue;
                                }
                            }
                        }
                    }

                    throw new Exception ($"Invalid line in {Utility.TargetsFileName}: {xLine}");
                }

                if (xTargets.Where (x => Directory.Exists (x.DirectoryPath)).Count () == 0)
                    throw new Exception ($"No valid targets found in {Utility.TargetsFileName}.");

                var xSortedTargets = xTargets.OrderBy (x => x.DirectoryPath, StringComparer.OrdinalIgnoreCase).ToArray ();

                Utility.AddLogLine ("[Targets]");

                Utility.AddLogLine (string.Join (Environment.NewLine, xSortedTargets.
                    Where (x => Directory.Exists (x.DirectoryPath)).
                    Select (x => $"{x.DirectoryPath} | {x.Options}")));

                foreach (var xTarget in xSortedTargets)
                {
                    if (Directory.Exists (xTarget.DirectoryPath) == false)
                        continue;

                    Utility.AddLogLine ();
                    Utility.AddLogLine ($"[{xTarget.DirectoryPath}]");
                    Utility.AddLogLine ("Options: " + xTarget.Options);

                    Console.WriteLine ($"Cleaning: {xTarget.DirectoryPath}");

                    bool xDeletesAllFiles = xTarget.Options.HasFlag (CleaningOptions.DeleteAllFiles),
                         xDeletesEmptyDirectories = xTarget.Options.HasFlag (CleaningOptions.DeleteEmptyDirectories);

                    DateTime x24HoursAgoUtc = DateTime.UtcNow.AddHours (-24);

                    int xHandledFileCount = 0,
                        xHandledDirectoryCount = 0,
                        xDeletedFileCount = 0,
                        xDeletedDirectoryCount = 0;

                    string _GetHandledStatistics () => $"Handled {xHandledFileCount} files and {xHandledDirectoryCount} directories.";

                    string _GetDeletedStatistics () => $"Deleted {xDeletedFileCount} files and {xDeletedDirectoryCount} directories.";

                    void _DisplayStatistics () => Console.Write ($"\r{_GetHandledStatistics ()} {_GetDeletedStatistics ()}");

                    void _HandleDirectory (DirectoryInfo directory, int depth)
                    {
                        try
                        {
                            foreach (DirectoryInfo xSubdirectory in directory.GetDirectories ().OrderBy (x => x.Name, StringComparer.OrdinalIgnoreCase))
                                _HandleDirectory (xSubdirectory, depth + 1);

                            foreach (FileInfo xFile in directory.GetFiles ().OrderBy (x => x.Name, StringComparer.OrdinalIgnoreCase))
                            {
                                try
                                {
                                    if (xDeletesAllFiles ||
                                       (xDeletesAllFiles == false && xFile.LastWriteTimeUtc < x24HoursAgoUtc)) // LastWriteTimeUtc may throw an exception.
                                    {
                                        xFile.Attributes = FileAttributes.Normal;
                                        xFile.Delete ();

                                        Utility.AddLogLine ($"Deleted file: {xFile.FullName}");

                                        xDeletedFileCount ++;
                                        _DisplayStatistics ();
                                    }
                                }

                                catch
                                {
                                    Utility.AddLogLine ($"Error deleting file: {xFile.FullName}");
                                    // Console.WriteLine ($"\rError deleting file: {xFile.FullName}"); // Doesnt seem like a big deal.
                                }

                                finally
                                {
                                    xHandledFileCount ++;
                                    _DisplayStatistics ();
                                }
                            }

                            if (depth > 0 && xDeletesEmptyDirectories)
                            {
                                try
                                {
                                    if (directory.GetFileSystemInfos ().Length == 0) // GetFileSystemInfos may throw an exception.
                                    {
                                        directory.Attributes = FileAttributes.Directory;
                                        directory.Delete ();

                                        Utility.AddLogLine ($"Deleted directory: {directory.FullName}");

                                        xDeletedDirectoryCount ++;
                                        _DisplayStatistics ();
                                    }
                                }

                                catch
                                {
                                    Utility.AddLogLine ($"Error deleting directory: {directory.FullName}");
                                    // Console.WriteLine ($"\rError deleting directory: {directory.FullName}");
                                }
                            }
                        }

                        catch
                        {
                            Utility.AddLogLine ($"Error handling directory: {directory.FullName}");
                            // Console.WriteLine ($"\rError handling directory: {directory.FullName}");
                        }

                        finally
                        {
                            xHandledDirectoryCount ++;
                            _DisplayStatistics ();
                        }
                    }

                    DirectoryInfo xTargetDirectory = new (xTarget.DirectoryPath);
                    _HandleDirectory (xTargetDirectory, depth: 0);

                    Utility.AddLogLine (_GetHandledStatistics ());
                    Utility.AddLogLine (_GetDeletedStatistics ());

                    _DisplayStatistics ();
                    Console.WriteLine ();
                }
            }

            catch (Exception xException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine (xException.ToString ());
                Console.ResetColor ();
            }

            finally
            {
                if (Utility.Logs.Count > 0)
                {
                    Directory.CreateDirectory (Utility.LogsDirectoryPath);
                    File.WriteAllLines (Utility.LogFilePath, Utility.Logs, Encoding.UTF8);
                    Console.WriteLine ($"Logs saved to: {Utility.LogFilePath}");
                }

                Console.Write ("Press any key to exit: ");
                Console.ReadKey (true);
                Console.WriteLine ();
            }
        }
    }
}
