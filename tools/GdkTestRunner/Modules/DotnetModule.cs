using System;
using System.IO;
using GdkTestRunner.Model;
using Newtonsoft.Json.Linq;
using NLog;

namespace GdkTestRunner.Modules
{
    public class DotnetModule : BaseModule
    {
        public override string JsonModuleIdentifier => "dotnet";
        public override string Name { get; }

        private readonly string dotnetProjectPath;
        private readonly string testResultsPath;

        private readonly Logger logger;

        public DotnetModule()
        {
        }

        public DotnetModule(GdkTestRunnerOptions options, JToken jsonContext) : base(options, jsonContext)
        {
            var parsedJson = jsonContext.ToObject<DotnetModuleDefinition>();

            Name = parsedJson.Name;
            dotnetProjectPath = Path.Combine(Path.GetDirectoryName(options.ConfigurationFilePath),
                parsedJson.DotnetDefinition.DotnetProjectPath);

            var formattedName = Formatter.TitleCaseToKebabCase(parsedJson.Name);
            testResultsPath = GetLogFilePath($"{formattedName}-results.xml");

            logger = LogManager.GetCurrentClassLogger();
        }

        protected override bool InternalRun()
        {
            var arguments = new[]
            {
                "test",
                $"--logger:\"nunit;LogFilePath={testResultsPath}\"",
                $"\"{dotnetProjectPath}\""
            };

            var args = string.Join(' ', arguments);

            logger.Debug($"Running: dotnet {args}");

            try
            {
                if (!RunProcess("dotnet", args))
                {
                    logger.Error($"{Name} exited with non-zero exit code" +
                        $"Check {testResultsPath} for more info.");
                    return false;
                }
            }
            catch (Exception e)
            {
                logger.Error($"Exception thrown while testing: {e.Message}\n{e.InnerException.Message}");
                return false;
            }

            return true;
        }

        protected override void PrintHelp()
        {
            throw new NotImplementedException();
        }
    }
}
