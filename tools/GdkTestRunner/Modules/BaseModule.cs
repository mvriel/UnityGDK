using System;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json.Linq;

namespace GdkTestRunner.Modules
{
    public abstract class BaseModule
    {
        public abstract string Name { get; }

        protected BaseModule(GdkTestRunnerOptions options, JToken jsonContext)
        {
        }

        public bool Run()
        {
            BeforeRun();
            var result = InternalRun();
            AfterRun();

            return result;
        }

        protected string GetLogFilePath(string logFileName)
        {
            return $"{Environment.CurrentDirectory}/logs/{logFileName}";
        }

        protected bool RunProcess(string exe, string args)
        {
            using (var process = Process.Start(exe, args))
            {
                if (process == null)
                {
                    throw new Exception($"Failed to start process \"{exe} {args}\".");
                }

                process.WaitForExit();
                return process.ExitCode == 0;
            }
        }

        protected virtual void BeforeRun()
        {
        }

        protected abstract bool InternalRun();

        protected virtual void AfterRun()
        {
        }

        protected abstract void PrintHelp();
    }

    public class TestModuleIdentifierAttribute : Attribute
    {
        public string Identifier;

        public TestModuleIdentifierAttribute(string identifier)
        {
            Identifier = identifier;
        }
    }
}
