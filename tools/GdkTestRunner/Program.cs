using System;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace GdkTestRunner
{
    class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                var options = GdkTestRunnerOptions.ParseArguments(args);

                if (options.ShouldShowHelp)
                {
                    Console.WriteLine(options.HelpString);
                    return;
                }

                options.Validate();

                SetupLogger(options);
                var runner = new TestRunner(options);
                runner.Run();
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                Environment.Exit(1);
            }
        }

        private static void SetupLogger(GdkTestRunnerOptions options)
        {
            var logConfig = new LoggingConfiguration();

            if (options.LogFilePath != null)
            {
                var logFileTarget = new FileTarget("log-file")
                {
                    FileName = options.LogFilePath
                };

                logConfig.AddRule(LogLevel.Trace, LogLevel.Fatal, logFileTarget);
            }

            var consoleTarget = new ConsoleTarget("log-console");
            var minimumLogLevel = options.ShouldLogVerbose ? LogLevel.Debug : LogLevel.Info;
            logConfig.AddRule(minimumLogLevel, LogLevel.Fatal, consoleTarget);

            LogManager.Configuration = logConfig;
        }
    }
}
