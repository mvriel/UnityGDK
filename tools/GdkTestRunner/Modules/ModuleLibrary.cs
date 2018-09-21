using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace GdkTestRunner.Modules
{
    public static class ModuleLibrary
    {
        private static Dictionary<string, Type> moduleTypeLibrary = new Dictionary<string, Type>();

        static ModuleLibrary()
        {
            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes());

            var moduleTypes = types.Where(type => typeof(BaseModule).IsAssignableFrom(type) && !type.IsAbstract);

            foreach (var moduleType in moduleTypes)
            {
                var instance = (BaseModule) Activator.CreateInstance(moduleType);

                moduleTypeLibrary[instance.JsonModuleIdentifier] = moduleType;
            }
        }

        public static BaseModule CreateModuleFromType(string type, JToken jsonContext, GdkTestRunnerOptions options)
        {
            if (!moduleTypeLibrary.TryGetValue(type, out var moduleType))
            {
                throw new ArgumentException($"Cannot find a module with JSON identifier: {type}");
            }

            return (BaseModule) Activator.CreateInstance(moduleType, options, jsonContext);
        }
    }
}
