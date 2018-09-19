using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GdkTestRunner.Modules;
using Newtonsoft.Json.Linq;
using NLog;

namespace GdkTestRunner
{
    public class TestRunner
    {
        private GdkTestRunnerOptions options;
        private Logger logger;
        private List<BaseModule> testModules = new List<BaseModule>();

        public TestRunner(GdkTestRunnerOptions options)
        {
            this.options = options;
            logger = LogManager.GetCurrentClassLogger();
        }

        public void Run()
        {
            PopulateModules();

            foreach (var module in testModules)
            {
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
                var moduleInstance = ModuleLibrary.CreateModuleFromType(module["type"].ToString(), module);
                logger.Info($"Found {moduleInstance.Name}.");
                testModules.Add(moduleInstance);
            }

            logger.Info($"Test module discovery complete. Found: {modules.Count} modules.");
        }
    }
}
