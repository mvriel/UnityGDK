using System;
using System.IO;
using Newtonsoft.Json.Linq;

namespace GdkTestRunner.Modules
{
    public abstract class BaseModule
    {
        /// <summary>
        ///     A string that represents the JSON identifier for this module.
        /// </summary>
        public abstract string JsonModuleIdentifier { get; }

        public abstract string Name { get; }

        protected BaseModule()
        {
        }

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

        protected abstract void BeforeRun();
        protected abstract bool InternalRun();
        protected abstract void AfterRun();

        protected abstract void PrintHelp();
    }
}
