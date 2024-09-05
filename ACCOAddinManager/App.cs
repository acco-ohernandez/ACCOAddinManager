using System.Diagnostics;
using System.IO;
using System.Text;

namespace ACCOAddinManager
{
    internal class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication app)
        {
            WriteLog("  ===================== ACCOAddinManager =========================================.", LogLevel.Info);
            bool addinHasRunToday = CheckIfAddinManagerHasRunToday();
            if (addinHasRunToday)
            {
                return Result.Cancelled;
            }


            // Run the ACCOAddinManager
            var res = RunAccoAddinManager();
            if (!res)
            {
                WriteLog("No addins updated.", LogLevel.Error);
                return Result.Cancelled;
            }

            return Result.Succeeded;
        }

        private bool CheckIfAddinManagerHasRunToday()
        {
            // Define the flag file path
            string nameSpace = this.GetType().Namespace;
            var Userprofile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\AppData\Local\Temp";
            var flagDir = Path.Combine(Userprofile, nameSpace);
            var flagFile = Path.Combine(flagDir, $"{nameSpace}.flag");

            try
            {
                // Check if the flag file exists
                if (File.Exists(flagFile))
                {
                    // Check if the file was created today
                    if (IsFlagFileCreatedToday(flagFile))
                    {
                        //Debug.WriteLine("The ACCOAddinManager has already run today.");
                        WriteLog("Revit started again, but the ACCOAddinManager has already run today.", LogLevel.Warning);
                        return true;
                    }
                    else
                    {
                        //Debug.WriteLine("Flag file exists but was not created today. Proceeding with execution.");
                        WriteLog("Flag file exists but was not created today. Proceeding with execution.", LogLevel.Warning);
                    }
                }
                else
                {
                    //Debug.WriteLine("Flag file does not exist. Proceeding with execution.");
                    WriteLog("Flag file does not exist. Proceeding with execution.", LogLevel.Warning);
                }

                // Create or update the flag file
                CreateFlagFile(flagFile);
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred while checking or creating the flag file: {ex.Message}");
                WriteLog($"An error occurred while checking or creating the flag file: {ex.Message}", LogLevel.Error);
                return false;
            }
        }

        private bool IsFlagFileCreatedToday(string flagFile)
        {
            try
            {
                return File.GetCreationTime(flagFile).Date == DateTime.Today;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred while checking the flag file creation date: {ex.Message}");
                WriteLog($"An error occurred while checking the flag file creation date: {ex.Message}", LogLevel.Error);
                return false;
            }
        }

        private void CreateFlagFile(string flagFile)
        {
            try
            {
                // Get the directory of the flag file
                string flagFileDirectory = Path.GetDirectoryName(flagFile);
                if (!string.IsNullOrEmpty(flagFileDirectory))
                {
                    // Ensure the directory exists
                    Directory.CreateDirectory(flagFileDirectory);
                }

                // Check if the file already exists
                bool fileExists = File.Exists(flagFile);

                // Create or overwrite the flag file
                using (var fs = File.Create(flagFile))
                {
                    // Optionally, write the current date and time to the file
                    using (StreamWriter writer = new StreamWriter(fs))
                    {
                        writer.WriteLine($"Flag file created/updated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                    }
                }

                // Log whether the file was created or updated
                if (fileExists)
                {
                    Debug.WriteLine("Flag file was updated successfully.");
                    WriteLog("Flag file was updated successfully.", LogLevel.Info);
                }
                else
                {
                    Debug.WriteLine("Flag file was created successfully.");
                    WriteLog("Flag file was created successfully.", LogLevel.Info);
                }
            }
            catch (UnauthorizedAccessException uex)
            {
                Debug.WriteLine($"Access error while creating the flag file: {uex.Message}");
                WriteLog($"Access error while creating the flag file: {uex.Message}", LogLevel.Error);
            }
            catch (IOException ioex)
            {
                Debug.WriteLine($"IO error while creating the flag file: {ioex.Message}");
                WriteLog($"IO error while creating the flag file: {ioex.Message}", LogLevel.Error);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An unexpected error occurred while creating the flag file: {ex.Message}");
                WriteLog($"An unexpected error occurred while creating the flag file: {ex.Message}", LogLevel.Error);
            }
        }



        public bool RunAccoAddinManager()
        {
            WriteLog("This is the Addin Manager", LogLevel.Info);
            string ACCOAddinManagerCsv = @"C:\ProgramData\Autodesk\Revit\Addins\ACCOAddinManager.csv";
            if (!File.Exists(ACCOAddinManagerCsv))
            {
                WriteLog("The ACCOAddinManager.csv file does not exist.", LogLevel.Error);
                Debug.WriteLine("The ACCOAddinManager.csv file does not exist.");
                // Create  method that creates the CSV file and prompts the user to populate it
                CreateACCOAddinManagerCsv();

                return false;
            }

            var addinsList = AccoAddinsCsv.ParseCsv(ACCOAddinManagerCsv); //parse the CSV file and return a list of AccoAddinsCsv objects
            ShowCsvList(addinsList); //show the list of addins from the CSV file in the debug output

            var listOfAddinsUpdated = ListOfAddinsUpdated(addinsList);//create a new list of addins that have been updated
            if (listOfAddinsUpdated.Count == 0)
            {
                //Debug.WriteLine("No addins have been updated.");
                WriteLog("No addins have been updated.", LogLevel.Info);
                return false;
            }
            TaskDialog.Show("ACCOAddinManager", $"The following addins have been updated: {string.Join(", ", listOfAddinsUpdated)}");

            return true;
        }

        private void CreateACCOAddinManagerCsv()
        {
            // Create the CSV file
            string ACCOAddinManagerCsv = @"C:\ProgramData\Autodesk\Revit\Addins\ACCOAddinManager.csv";
            try
            {
                // Create the CSV file
                using (StreamWriter sw = File.CreateText(ACCOAddinManagerCsv))
                {
                    sw.WriteLine("LocalAddinFile,LocalDllFile,ServerAddinFile,ServerDllFile");
                    sw.WriteLine(@"C:\ProgramData\Autodesk\Revit\Addins\ACCOAddinManager\SAMPLE_addin_FILE.addin, C:\ProgramData\Autodesk\Revit\Addins\ACCOAddinManager\SAMPLE_dll_File.dll, \\server\share\SAMPLE_addin_FILE.addin, \\server\share\SAMPLE_dll_File.dll");
                }
#if REVIT2025
                using (Process process = new Process())
                {
                    process.StartInfo.UseShellExecute = true;
                    process.StartInfo.FileName = ACCOAddinManagerCsv;
                    process.Start();
                }
#else
                Process.Start("notepad.exe", ACCOAddinManagerCsv);
#endif






                WriteLog("The ACCOAddinManager.csv file has been created.", LogLevel.Info);
                TaskDialog.Show("ACCOAddinManager", $"The ACCOAddinManager.csv file has been created. Please populate it with the addins you want to manage.\n{ACCOAddinManagerCsv}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred while creating the ACCOAddinManager.csv file: {ex.Message}");
                WriteLog($"An error occurred while creating the ACCOAddinManager.csv file: {ex.Message}", LogLevel.Error);
            }
        }

        private List<string> ListOfAddinsUpdated(List<AccoAddinsCsv> addinsList)
        {
            int newAddinCounter = 0;
            int updateAddinCounter = 0;
            int newDLLCounter = 0;
            int updateDLLCounter = 0;
            List<string> listOfAddinsUpdated = new List<string>();
            StringBuilder updatesString = new StringBuilder();
            // For each addin in the list, check if the local file exists and if it is up to date with the server file
            foreach (var addin in addinsList)
            {
                updatesString.Clear();
                // Check if the addin file exists and if it is up to date with the server file
                if (!File.Exists(addin.LocalAddinFile))
                {
                    // Create the folder if it doesn't exist
                    string localFolder = Path.GetDirectoryName(addin.LocalAddinFile);
                    if (!Directory.Exists(localFolder))
                    {
                        Directory.CreateDirectory(localFolder);
                        WriteLog($"Created folder: {localFolder}", LogLevel.Info);
                    }
                    // Copy the server addin file to the local folder
                    File.Copy(addin.ServerAddinFile, addin.LocalAddinFile);
                    WriteLog($"New Addin: {Path.GetFileName(addin.LocalAddinFile)}", LogLevel.Info);
                    updatesString.Append($"New Addin: {Path.GetFileName(addin.LocalAddinFile)}::");
                    newAddinCounter++;
                }
                else
                {
                    // Check if the server file is newer than the local file
                    FileInfo localInfo = new FileInfo(addin.LocalAddinFile);
                    FileInfo serverInfo = new FileInfo(addin.ServerAddinFile);
                    // If the server file is newer, copy it to the local folder
                    if (serverInfo.LastWriteTime > localInfo.LastWriteTime)
                    {
                        File.Copy(addin.ServerAddinFile, addin.LocalAddinFile, true);
                        WriteLog($"Updated Addin: {Path.GetFileName(addin.LocalAddinFile)}", LogLevel.Info);
                        updatesString.Append($"Updated Addin: {Path.GetFileName(addin.LocalAddinFile)}::");
                        updateAddinCounter++;
                    }
                }
                // Check if the DLL file exists and if it is up to date with the server file
                if (!File.Exists(addin.LocalDllFile))
                {
                    string localFolder = Path.GetDirectoryName(addin.LocalDllFile);
                    // Create the folder if it doesn't exist
                    if (!Directory.Exists(localFolder))
                    {
                        Directory.CreateDirectory(localFolder);
                        WriteLog($"Created folder: {localFolder}", LogLevel.Info);
                    }
                    // Copy the server DLL file to the local folder
                    File.Copy(addin.ServerDllFile, addin.LocalDllFile);
                    WriteLog($"New DLL: {Path.GetFileName(addin.LocalDllFile)}", LogLevel.Info);
                    updatesString.Append($"New DLL: {Path.GetFileName(addin.LocalDllFile)}");
                    CopyDependentDlls(addin);
                    newDLLCounter++;
                }
                else
                {
                    // Check if the server DLL file is newer than the local DLL file
                    FileInfo localInfo = new FileInfo(addin.LocalDllFile);
                    FileInfo serverInfo = new FileInfo(addin.ServerDllFile);
                    // If the server DLL file is newer, copy it to the local folder
                    if (serverInfo.LastWriteTime > localInfo.LastWriteTime)
                    {
                        File.Copy(addin.ServerDllFile, addin.LocalDllFile, true);
                        WriteLog($"Updated DLL: {Path.GetFileName(addin.LocalDllFile)}", LogLevel.Info);
                        updatesString.Append($"Updated DLL: {Path.GetFileName(addin.LocalDllFile)}");
                        // Check if there are other dlls in the same directory of the ServerDllFile and copy them to the local directory if they are newer
                        CopyDependentDlls(addin);
                        string addinName = Path.GetFileName(addin.LocalDllFile);
                        listOfAddinsUpdated.Add(addinName);
                        updateDLLCounter++;
                    }
                }
            }
            //Debug.WriteLine($"New Addins: {newCounter}");
            WriteLog($"New DLLAddins: {newDLLCounter}", LogLevel.Info);
            return listOfAddinsUpdated;
        }

        private void CopyDependentDlls(AccoAddinsCsv addin)
        {
            try
            {
                // Check if there are other DLLs in the same directory of the ServerDllFile and copy them to the local directory
                string serverDllFolder = Path.GetDirectoryName(addin.ServerDllFile);
                string localDllFolder = Path.GetDirectoryName(addin.LocalDllFile);

                if (serverDllFolder == null || localDllFolder == null)
                {
                    throw new DirectoryNotFoundException("The specified directory path is invalid.");
                }

                // Ensure the local directory exists
                Directory.CreateDirectory(localDllFolder);

                string[] serverDllFiles = Directory.GetFiles(serverDllFolder, "*.dll");

                foreach (var serverDllFile in serverDllFiles)
                {
                    FileInfo serverDllInfo = new FileInfo(serverDllFile);
                    string curServerDll = Path.GetFileName(serverDllFile);
                    string curLocalDll = Path.Combine(localDllFolder, curServerDll);

                    try
                    {
                        if (!File.Exists(curLocalDll))
                        {
                            File.Copy(serverDllFile, curLocalDll);
                            Debug.WriteLine($"Copied: {curServerDll} to {localDllFolder}");
                            WriteLog($"Copied: {curServerDll} to {localDllFolder}", LogLevel.Info);
                        }
                        else
                        {
                            FileInfo localInfo = new FileInfo(curLocalDll);
                            if (serverDllInfo.LastWriteTime > localInfo.LastWriteTime)
                            {
                                File.Copy(serverDllFile, curLocalDll, true);
                                Debug.WriteLine($"Updated: {curServerDll} in {localDllFolder}");
                                WriteLog($"Updated: {curServerDll} in {localDllFolder}", LogLevel.Info);
                            }
                            else
                            {
                                Debug.WriteLine($"Skipped: {curServerDll} (local version is up-to-date)");
                                WriteLog($"Skipped: {curServerDll} (local version is up-to-date)", LogLevel.Info);
                            }
                        }
                    }
                    catch (IOException ioEx)
                    {
                        Debug.WriteLine($"IO Error: Could not copy {curServerDll}. {ioEx.Message}");
                        WriteLog($"IO Error: Could not copy {curServerDll}. {ioEx.Message}", LogLevel.Error);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error: Could not copy {curServerDll}. {ex.Message}");
                        WriteLog($"Error: Could not copy {curServerDll}. {ex.Message}", LogLevel.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message}");
                WriteLog($"Error: {ex.Message}", LogLevel.Error);
            }
        }

        private void ShowCsvList(List<AccoAddinsCsv> addinsList)
        {
            WriteLog("List of Addins from the CSV file:", LogLevel.Info);
            foreach (var addin in addinsList)
            {
                Debug.WriteLine($"Local Addin File: {addin.LocalAddinFile}");
                Debug.WriteLine($"Local DLL File: {addin.LocalDllFile}");
                Debug.WriteLine($"Server Addin File: {addin.ServerAddinFile}");
                Debug.WriteLine($"Server DLL File: {addin.ServerDllFile}");
                Debug.WriteLine("");
                WriteLog($"Local Addin File: {addin.LocalAddinFile},Local DLL File: {addin.LocalDllFile},Server Addin File: {addin.ServerAddinFile},Server DLL File: {addin.ServerDllFile}", LogLevel.Info);
            }
        }

        public enum LogLevel
        {
            Info,
            Warning,
            Error,
            Debug // Only logged when in a debug build
        }

        public void WriteLog(string logMessage, LogLevel level = LogLevel.Info)
        {
            // Skip debug messages unless we're in a debug build
#if !DEBUG
            if (level == LogLevel.Debug)
            {
                return;
            }
#endif

            // Get the namespace name for folder and file naming
            var Userprofile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\AppData\Local\Temp";
            string nameSpace = this.GetType().Namespace;
            string logFolder = Path.Combine(Userprofile, nameSpace);
            string logFile = Path.Combine(logFolder, $"{nameSpace}_{DateTime.Today:yyyy-MM-dd}.log");

            try
            {
                // Create the log folder if it doesn't exist
                if (!Directory.Exists(logFolder))
                {
                    Directory.CreateDirectory(logFolder);
                }

                // Write the log message to the log file (creates the file if it doesn't exist)
                using (StreamWriter sw = File.AppendText(logFile))
                {
                    sw.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}]::{logMessage}");
                }

                // Manage log retention: keep only the last 10 days of logs
                string[] logFiles = Directory.GetFiles(logFolder, $"{nameSpace}_*.log");

                if (logFiles.Length > 10)
                {
                    var filesToDelete = logFiles
                        .Select(f => new FileInfo(f))
                        .OrderBy(f => f.CreationTime)
                        .Take(logFiles.Length - 10);

                    foreach (var file in filesToDelete)
                    {
                        file.Delete();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred while writing the log: {ex.Message}");
                WriteLog($"An error occurred while writing the log: {ex.Message}", LogLevel.Error);
            }
        }




        public Result OnShutdown(UIControlledApplication a)
        {
            return Result.Succeeded;
        }
    }

}
