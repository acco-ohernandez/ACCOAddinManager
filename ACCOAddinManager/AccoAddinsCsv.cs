using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ACCOAddinManager
{
    internal class AccoAddinsCsv
    {
        public string LocalAddinFile { get; set; }
        public string LocalDllFile { get; set; }
        public string ServerAddinFile { get; set; }
        public string ServerDllFile { get; set; }

        // Constructor to initialize the object with values
        public AccoAddinsCsv(string localAddinFile, string localDllFile, string serverAddinFile, string serverDllFile)
        {
            LocalAddinFile = localAddinFile;
            LocalDllFile = localDllFile;
            ServerAddinFile = serverAddinFile;
            ServerDllFile = serverDllFile;
        }

        // Method to parse the CSV file and return a list of AccoAddinsCsv objects
        public static List<AccoAddinsCsv> ParseCsv(string csvFilePath)
        {
            var addinFiles = new List<AccoAddinsCsv>();
            var lines = File.ReadAllLines(csvFilePath);

            // Skip the header line
            foreach (var line in lines.Skip(1))
            {
                // Split the line into columns
                var columns = line.Split(',');

                // Ensure there are exactly 4 columns in the CSV line
                if (columns.Length < 4)
                {
                    Debug.WriteLine($"Skipping line due to insufficient columns: {line}");
                    continue; // Skip this line if it doesn't have the expected number of columns
                }

                // Remove any quotes around the file paths and trim any excess spaces
                var localAddinFile = columns[0].Trim('"').Trim();
                var localDllFile = columns[1].Trim('"').Trim();
                var serverAddinFile = columns[2].Trim('"').Trim();
                var serverDllFile = columns[3].Trim('"').Trim();

                // Create a new AccoAddinsCsv object and add it to the list
                var addinFileInfo = new AccoAddinsCsv(localAddinFile, localDllFile, serverAddinFile, serverDllFile);
                addinFiles.Add(addinFileInfo);
            }

            return addinFiles;
        }
    }
}
