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

                        if (Directory.Exists (xDirectoryPath))
                        {
                            if (Enum.TryParse <CleaningOptions> (xParts [1].Trim (), ignoreCase: true, out var xOptions))
                            {
                                xTargets.Add ((xDirectoryPath, xOptions));
                                continue;
                            }
                        }
                    }

                    throw new Exception ($"Invalid line in {Utility.TargetsFileName}: {xLine}");
                }

                if (xTargets.Count == 0)
                    throw new Exception ($"No valid targets found in {Utility.TargetsFileName}.");
            }

            catch (Exception xException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine (xException.ToString ());
                Console.ResetColor ();
            }

            finally
            {
                Console.Write ("Press any key to exit: ");
                Console.ReadKey (true);
                Console.WriteLine ();
            }
        }
    }
}
