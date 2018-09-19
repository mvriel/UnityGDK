using System;
using System.Collections.Generic;
using System.IO;
using Mono.Options;

namespace GdkTestRunner
{
    public class GdkTestRunnerOptions
    {
        public string ConfigurationFilePath;

        public bool ShouldLogVerbose;
        public string LogFilePath;

        public bool ShouldShowHelp;
        public string HelpString;

        private GdkTestRunnerOptions()
        {
        }

        public void Validate()
        {
            if (ConfigurationFilePath == null)
            {
                throw new ArgumentException("The GDK test runner requires a configuration file to run.");
            }

            if (!File.Exists(ConfigurationFilePath))
            {
                throw new ArgumentException($"Could not find configuration file at: {ConfigurationFilePath}");
            }
        }

        public static GdkTestRunnerOptions ParseArguments(ICollection<string> args)
        {
            var options = new GdkTestRunnerOptions();

            var optionSet = new OptionSet
            {
                {
                    "config-file=",
                    "REQUIRED: The file path of the GDK test runner configuration file.",
                    s => options.ConfigurationFilePath = Path.GetFullPath(s)
                },
                {
                    "verbose-logging",
                    "If this flag is set, verbose logging will be set on.",
                    s => options.ShouldLogVerbose = s != null
                },
                {
                    "log-file=",
                    "The path to the Test Runner log file",
                    s => options.LogFilePath = Path.GetFullPath(s)
                },
                {
                    "h|help",
                    "Show help",
                    s => options.ShouldShowHelp = s != null
                }
            };

            optionSet.Parse(args);

            using (var stringWriter = new StringWriter())
            {
                optionSet.WriteOptionDescriptions(stringWriter);
                options.HelpString = stringWriter.ToString();
            }

            return options;
        }
    }
}
