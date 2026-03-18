using System.Text.Json;
using System.Text.Json.Serialization;

namespace CohesiveRP.Core.Lorebooks.Loaders.CCv3.BusinessObjects
{
    public class CCv3EntryExtensions
    {
        [JsonPropertyName("addMemo")]
        public bool AddMemo { get; set; }

        // Always null in the observed data.
        [JsonPropertyName("characterFilter")]
        public JsonElement? CharacterFilter { get; set; }

        [JsonPropertyName("depth")]
        public int Depth { get; set; }

        [JsonPropertyName("displayIndex")]
        public int DisplayIndex { get; set; }

        [JsonPropertyName("excludeRecursion")]
        public bool ExcludeRecursion { get; set; }

        [JsonPropertyName("probability")]
        public int Probability { get; set; }

        [JsonPropertyName("selectiveLogic")]
        public int SelectiveLogic { get; set; }

        [JsonPropertyName("useProbability")]
        public bool UseProbability { get; set; }

        [JsonPropertyName("weight")]
        public int Weight { get; set; }
    }
}
