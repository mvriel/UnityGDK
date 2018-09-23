using Newtonsoft.Json;

namespace GdkTestRunner.Model
{
    public class DotnetModuleDefinition : BaseModuleDefinition
    {
        [JsonProperty("parameters")] public DotnetSpecificDefinition DotnetDefinition;

        public struct DotnetSpecificDefinition
        {
            [JsonProperty("project_path")] public string DotnetProjectPath;
        }
    }
}
