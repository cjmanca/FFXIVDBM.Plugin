using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Security.Cryptography;

namespace GenerateVersionJSON
{
    class Program
    {
        static bool firstFile = true;
        static bool wasError = false;
        static System.IO.StreamWriter outFile;
        static string ignoreAssembly = @"GenerateVersionJSON.exe";
        static char[] charsToTrim = { '\\', '/' };
        
        static void Main(string[] args)
        {
            string mainAssembly = @"";
            string friendlyName = @"";
            string description = @"";
            string updateURL = @"";


            foreach (string arg in args)
            {
                string command = "";
                string value = "";

                string[] parts = arg.Split('=');

                if (parts.Length >= 2)
                {
                    command = parts[0].Trim();
                    value = parts[1].Trim('"').Replace('"', '\'');

                    switch (command.ToLowerInvariant())
                    {
                        case "name":
                            mainAssembly = value;
                            break;
                        case "friendlyname":
                            friendlyName = value;
                            break;
                        case "description":
                            description = value;
                            break;
                        case "url":
                            updateURL = value;
                            break;

                    }
                }
            }

            if (!File.Exists(mainAssembly + @".dll"))
            {
                Console.WriteLine("Make sure your current working directory is in the distribution base directory.\n");
                Console.WriteLine("Parameters:");
                Console.WriteLine(" name - The assembly name, without the extension.");
                Console.WriteLine(" friendlyname - The plugin name which will be displayed in the update tab.");
                Console.WriteLine(" description - The plugin description to be displayed in the update tab.");
                Console.WriteLine(" url - The base url for the distribution files including a trailing slash.");
                return;
            }

            FileVersionInfo mainAssemblyVersionInfo = FileVersionInfo.GetVersionInfo(mainAssembly + @".dll");

            outFile = new System.IO.StreamWriter("VERSION.json");

            string lines = @"{
    ""PluginInfo"": {
        ""Name"": """ + mainAssembly + @""",
        ""Version"": """ + mainAssemblyVersionInfo.FileVersion + @""",
        ""FriendlyName"": """ + friendlyName + @""",
        ""Description"": """ + description + @""",
        ""SourceURI"": """ + updateURL + @""",
        ""Files"": [";


            outFile.Write(lines);

            DirectoryInfo root = new DirectoryInfo(".");
            WalkDirectoryTree(root, "");


            lines = @"
        ]
    }
}
";
            outFile.Write(lines);
            outFile.Flush();

            outFile.Close();
            outFile.Dispose();

            if (wasError)
            {
                Console.ReadKey();
            }
        }

        static void WalkDirectoryTree(System.IO.DirectoryInfo root, string path)
        {
            System.IO.FileInfo[] files = null;
            System.IO.DirectoryInfo[] subDirs = null;

            path = path.Trim(charsToTrim);

            // First, process all the files directly under this folder 
            try
            {
                files = root.GetFiles("*.*");
            }
            // This is thrown if even one of the files requires permissions greater 
            // than the application provides. 
            catch (UnauthorizedAccessException e)
            {
                // This code just writes out the message and continues to recurse. 
                // You may decide to do something different here. For example, you 
                // can try to elevate your privileges and access the file again.
                Console.WriteLine(e.Message);
                wasError = true;
            }
            catch (System.IO.DirectoryNotFoundException e)
            {
                Console.WriteLine(e.Message);
                wasError = true;
            }

            if (files != null)
            {
                foreach (System.IO.FileInfo fi in files)
                {
                    // In this example, we only access the existing FileInfo object. If we 
                    // want to open, delete or modify the file, then 
                    // a try-catch block is required here to handle the case 
                    // where the file has been deleted since the call to TraverseTree().
                    //Console.WriteLine(fi.FullName);

                    if (fi.Name != ignoreAssembly && fi.Name != "VERSION.json")
                    {
                        if (!firstFile)
                        {
                            outFile.Write(@",");
                        }
                        firstFile = false;

                        string hash = GetFileHash(fi.DirectoryName + @"\" + fi.Name);

                        outFile.Write(@"
            {
                ""Name"": """ + fi.Name + @""",
                ""Location"": """ + path + @""",
                ""Checksum"": """ + hash + @"""
            }");
                    }
                }

                // Now find all the subdirectories under this directory.
                subDirs = root.GetDirectories();

                foreach (System.IO.DirectoryInfo dirInfo in subDirs)
                {
                    // Resursive call for each subdirectory.
                    WalkDirectoryTree(dirInfo, path + @"/" + dirInfo.Name);
                }
            }
        }



        /// <summary>
        /// Computes a file's checksum
        /// </summary>
        /// <param name="FilePath">Path to file</param>
        /// <returns>MD5 checksum for the file, or an empty string if the file doesn't exist.</returns>
        static string GetFileHash(string FilePath)
        {
            string FileHash = "";

            if (!File.Exists(FilePath))
            {
                return FileHash;
            }

            try
            {
                using (FileStream file = new FileStream(FilePath, FileMode.Open, FileAccess.Read))
                {
                    using (MD5 md5 = new MD5CryptoServiceProvider())
                    {
                        byte[] retVal = md5.ComputeHash(file);
                        file.Close();

                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < retVal.Length; i++)
                        {
                            sb.Append(retVal[i].ToString("x2"));
                        }
                        FileHash = sb.ToString();
                    }
                }
            }
            catch { }

            return FileHash;
        }
    }
}
