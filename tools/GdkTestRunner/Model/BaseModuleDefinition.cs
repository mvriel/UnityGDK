using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GdkTestRunner.Model
{
    public class BaseModuleDefinition
    {
        [JsonProperty("type")] public string Type;

        [JsonProperty("name")] public string Name;
    }
}
