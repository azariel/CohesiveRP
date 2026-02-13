using System.Reflection;
using CohesiveRP.Common.Diagnostics;

namespace CohesiveRP.Common.Utils
{
    public static class AssemblyUtils
    {
        // ********************************************************************
        //                            Public
        // ********************************************************************
        public static void LoadAssemblyFilesToMemory(string[] assemblyFilesPath)
        {
            if (assemblyFilesPath == null)
                return;

            foreach (string _FilePath in assemblyFilesPath)
            {
                if (!File.Exists(_FilePath))
                    throw new Exception($"B729FD97-6217-4D40-AC70-E73C4BE18235 - Dependent custom library file [{_FilePath}] doesn't exists. Aborting. ExecutingPath=[{Environment.CurrentDirectory}]");
            }

            foreach (string _FilePath in assemblyFilesPath)
            {
                try
                {
                    // By using fileinfo, we allow relative path as well
                    FileInfo _FileInfo = new FileInfo(_FilePath);
                    Assembly.LoadFrom(_FileInfo.FullName);

                } catch (Exception _Exception)
                {
                    LoggingManager.LogToFile("38728BD0-BEB7-4082-8DA9-4097A5E7E810", $"Dependent custom library file [{_FilePath}] doesn't exists. Aborting.", _Exception);
                    throw new Exception($"2F8BBF34-1269-4689-AFE0-FF0802D44A0A - Dll [{_FilePath}] could not be loaded.");
                }
            }
        }
    }
}
