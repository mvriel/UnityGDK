using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GdkTestRunner.Modules;
using Newtonsoft.Json.Linq;

namespace GdkTestRunner
{
    public class TestRunner
    {
        private GdkTestRunnerOptions options;
        private List<BaseModule> testModules = new List<BaseModule>();

        public TestRunner(GdkTestRunnerOptions options)
        {
            this.options = options;
        }

        public void Run()
        {
            if (options.ShouldShowHelp)
            {
                Console.WriteLine(options.HelpString);
                return;
            }

            // TODO: Setup logger.

            PopulateModules();

            foreach (var module in testModules)
            {
                // TODO: Record failures.
                module.Run();
            }
        }

        private void PopulateModules()
        {
            var config = File.ReadAllText(options.ConfigurationFilePath);
            var configJson = JObject.Parse(config);

            var modules = configJson["modules"].Children().ToList();

            foreach (var module in modules)
            {
                testModules.Add(ModuleLibrary.CreateModuleFromType(module["type"].ToString(), module));
            }
        }
    }
}
