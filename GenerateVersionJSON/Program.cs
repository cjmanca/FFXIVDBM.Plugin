using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

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
            string mainAssembly = @"FFXIVDBM.Plugin";
            string updateURL = @"https://github.com/cjmanca/FFXIVDBM.Plugin/raw/master/distribution";


            FileVersionInfo mainAssemblyVersionInfo = FileVersionInfo.GetVersionInfo(mainAssembly + @".dll");

            outFile = new System.IO.StreamWriter("VERSION.json");

            string lines = @"{
    ""PluginInfo"": {
        ""Name"": """ + mainAssembly + @""",
        ""Version"": """ + mainAssemblyVersionInfo.FileVersion + @""",
        ""Files"": [";


            outFile.Write(lines);

            DirectoryInfo root = new DirectoryInfo(".");
            WalkDirectoryTree(root, "");


            lines = @"
        ],
        ""SourceURI"": """ + updateURL + @"""
    }
}
";
            outFile.Flush();
            outFile.Write(lines);

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

                    if (fi.Name != ignoreAssembly)
                    {
                        if (!firstFile)
                        {
                            outFile.Write(@",");
                        }
                        firstFile = false;

                        outFile.Write(@"
            {
                ""Name"": """ + fi.Name + @""",
                ""Location"": """ + path + @"""
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
    }
}
