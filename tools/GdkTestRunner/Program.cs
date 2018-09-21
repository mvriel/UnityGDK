using System;
using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;

namespace GdkTestRunner
{
    class Program
    {
        public static int Main(string[] args)
        {
            try
            {
                var options = GdkTestRunnerOptions.ParseArguments(args);

                if (options.ShouldShowHelp)
                {
                    Console.WriteLine(options.HelpString);
                    return 0;
                }

                options.Validate();

                SetupLogger(options);
                var runner = new TestRunner(options);
                return runner.Run() ? 0 : 1;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                return 1;
            }
        }

        private static void SetupLogger(GdkTestRunnerOptions options)
        {
            var logConfig = new LoggingConfiguration();

            if (options.LogFilePath != null)
            {
                var logFileTarget = new FileTarget("log-file")
                {
                    FileName = options.LogFilePath,
                    Layout = @"[${time}] ${logger} - ${message}"
                };

                logConfig.AddRule(LogLevel.Trace, LogLevel.Fatal, logFileTarget);
            }

            var consoleTarget = new ConsoleTarget("log-console")
            {
                Layout = @"[${time}] ${logger} - ${message}"
            };
            var minimumLogLevel = options.ShouldLogVerbose ? LogLevel.Debug : LogLevel.Info;
            logConfig.AddRule(minimumLogLevel, LogLevel.Fatal, consoleTarget);

            LogManager.Configuration = logConfig;
        }
    }
}
