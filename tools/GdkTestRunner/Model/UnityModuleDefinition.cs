using Newtonsoft.Json;

namespace GdkTestRunner.Model
{
    public class UnityModuleDefinition : BaseModuleDefinition
    {
        [JsonProperty("parameters")] public UnitySpecificDefinition UnityDefinition;

        public struct UnitySpecificDefinition
        {
            [JsonProperty("project_path")] public string UnityProjectPath;

            [JsonProperty("test_platform")] public string TestPlatform;
        }
    }
}
