using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GdkTestRunner.Modules;
using Newtonsoft.Json;
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


            var logsDir = $"{Environment.CurrentDirectory}/logs/";
            if (Directory.Exists(logsDir))
            {
                Directory.Delete(logsDir, true);
            }

            Directory.CreateDirectory(logsDir);
        }

        public bool Run()
        {
            PopulateModules();
            var success = true;

            foreach (var module in testModules)
            {
                logger.Info($"Starting {module.Name}");
                if (!module.Run())
                {
                    logger.Error($"{module.Name} finished with errors!");
                    success = false;
                }
                else
                {
                    logger.Info($"Finished {module.Name} successfully.");
                }
            }

            if (success)
            {
                logger.Info("All tests finished successfully!");
            }
            else
            {
                logger.Error("One or more tests failed.");
            }

            return success;
        }

        private void PopulateModules()
        {
            var config = File.ReadAllText(options.ConfigurationFilePath);
            try
            {
                var configJson = JObject.Parse(config);
                var modules = configJson["modules"].Children().ToList();

                foreach (var module in modules)
                {
                    var moduleInstance = ModuleLibrary.CreateModuleFromType(module["type"].ToString(), module, options);
                    logger.Info($"Found {moduleInstance.Name}.");
                    testModules.Add(moduleInstance);
                }

                logger.Info($"Test module discovery complete. Found: {modules.Count} module(s).");
            }
            catch (JsonReaderException e)
            {
                logger.Fatal($"Could not read json at {options.ConfigurationFilePath}. {e.Message}");
            }
            catch (Exception e)
            {
                logger.Fatal(e.Message);
                logger.Fatal(e.InnerException.Message);
            }
        }
    }
}
