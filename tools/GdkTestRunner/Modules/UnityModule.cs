using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using GdkTestRunner.Model;
using Newtonsoft.Json.Linq;
using NLog;
using UnityPaths;

namespace GdkTestRunner.Modules
{
    [TestModuleIdentifier("unity")]
    public class UnityModule : BaseModule
    {
        public override string Name { get; }

        private readonly string unityProjectPath;
        private readonly string unityTestPlatform;
        private readonly string logfilePath;
        private readonly string testResultsPath;

        private readonly Logger logger;

        public UnityModule(GdkTestRunnerOptions options, JToken jsonContext) : base(options, jsonContext)
        {
            var parsedJson = jsonContext.ToObject<UnityModuleDefinition>();

            Name = parsedJson.Name;
            unityProjectPath = Path.Combine(Path.GetDirectoryName(options.ConfigurationFilePath),
                parsedJson.UnityDefinition.UnityProjectPath);
            unityTestPlatform = parsedJson.UnityDefinition.TestPlatform;

            if (!ValidateTestPlatform())
            {
                throw new ArgumentException($"Unknown test platform: {unityTestPlatform}");
            }

            var formattedName = Formatter.TitleCaseToKebabCase(parsedJson.Name);

            logfilePath = GetLogFilePath($"{formattedName}-{unityTestPlatform}.log");
            testResultsPath = GetLogFilePath($"{formattedName}-{unityTestPlatform}-results.xml");

            logger = LogManager.GetCurrentClassLogger();
        }

        protected override void BeforeRun()
        {
            CleanLibraryAndTempFolders();
        }

        protected override bool InternalRun()
        {
            var currentDirectory = Environment.CurrentDirectory;
            Environment.CurrentDirectory = unityProjectPath;

            var unityPath = GetUnityExePath(Paths.TryGetUnityPath());
            var arguments = new[]
            {
                "-batchmode",
                "-projectPath", $"\"{unityProjectPath}\"",
                "-runTests",
                "-testPlatform", $"{unityTestPlatform}",
                "-logfile", $"\"{logfilePath}\"",
                "-testResults", $"\"{testResultsPath}\""
            };

            var args = string.Join(' ', arguments);

            logger.Debug($"Running: {unityPath} {args}");

            try
            {
                if (!RunProcess(unityPath, args))
                {
                    logger.Error($"{Name} exited with a non-zero exit code. " +
                        $"Check {logfilePath} and {testResultsPath} for more info.");
                    return false;
                }
            }
            catch (Exception e)
            {
                logger.Error($"Exception thrown while testing: {e.Message}\n{e.InnerException.Message}");
                return false;
            }
            finally
            {
                Environment.CurrentDirectory = currentDirectory;
            }

            return true;
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

            logger.Debug($"Cleaning {libraryFolder}");
            logger.Debug($"Cleaning {tempFolder}");

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
            string relativeExePath;

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

        private bool ValidateTestPlatform()
        {
            return unityTestPlatform == "editmode" || unityTestPlatform == "playmode";
        }
    }
}
