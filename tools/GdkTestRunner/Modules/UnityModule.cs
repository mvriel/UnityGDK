using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Newtonsoft.Json.Linq;
using UnityPaths;

namespace GdkTestRunner.Modules
{
    public class UnityModule : BaseModule
    {
        private enum UnityTestPlatform
        {
            EDITMODE = 0,
            PLAYMODE = 1
        }

        public override string JsonModuleIdentifier => "unity";

        private readonly string unityProjectPath;
        private readonly UnityTestPlatform unityTestPlatform;
        private readonly string logfilePath;
        private readonly string testResultsPath;

        private string unityTestPlatformStr => unityTestPlatform == UnityTestPlatform.EDITMODE
            ? "editmode"
            : "playmode";


        public UnityModule(JToken jsonContext) : base(jsonContext)
        {
            // TODO: Parse JSON
        }

        protected override void BeforeRun()
        {
            CleanLibraryAndTempFolders();
        }

        protected override bool InternalRun()
        {
            var currentDirectory = Environment.CurrentDirectory;
            Environment.CurrentDirectory = unityProjectPath;

            try
            {
                var unityPath = GetUnityExePath(Paths.TryGetUnityPath());
                var arguments = new[]
                {
                    "-batchmode",
                    "-projectPath", $"\"{unityProjectPath}\"",
                    "-runTests",
                    "-testPlatform", $"{unityTestPlatformStr}",
                    "-logfile", $"\"{logfilePath}\"",
                    "-testResults", $"\"{testResultsPath}\""
                };

                using (var process = Process.Start(unityPath, string.Join(' ', arguments)))
                {
                    if (process == null)
                    {
                        // TODO: Log and print error.
                        return false;
                    }

                    process.WaitForExit();

                    if (process.ExitCode != 0)
                    {
                        // TODO: Log error.
                    }

                    return process.ExitCode == 0;
                }
            }
            catch (Exception e)
            {
                // TODO: Print error.
                return false;
            }
            finally
            {
                Environment.CurrentDirectory = currentDirectory;
            }
        }

        protected override void AfterRun()
        {
            CleanLibraryAndTempFolders();
        }

        protected override void PrintHelp()
        {
            throw new NotImplementedException();
        }

        private void CleanLibraryAndTempFolders()
        {
            var libraryFolder = Path.Combine(unityProjectPath, "Library");
            var tempFolder = Path.Combine(unityProjectPath, "Temp");

            if (Directory.Exists(libraryFolder))
            {
                Directory.Delete(libraryFolder, true);
            }

            if (Directory.Exists(tempFolder))
            {
                Directory.Delete(tempFolder, true);
            }
        }

        private string GetUnityExePath(string unityEditorFolderPath)
        {
            var relativeExePath = string.Empty;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                relativeExePath = "Unity.app/Contents/MacOS/Unity";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                relativeExePath = "Editor\\Unity.exe";
            }
            else
            {
                throw new Exception($"Platform '{RuntimeInformation.OSDescription}' is unsupported.");
            }

            return Path.Combine(unityEditorFolderPath, relativeExePath);
        }
    }
}
