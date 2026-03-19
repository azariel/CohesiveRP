using System.Text.Json.Serialization;
using CohesiveRP.Common;

namespace CohesiveRP.Storage.DataAccessLayer.Lorebooks.BusinessObjects
{
    public class LorebookEntry
    {
        [JsonPropertyName("keys")]
        public List<string> Keys { get; init; } = [];

        [JsonPropertyName("content")]
        public string Content { get; init; } = string.Empty;

        [JsonPropertyName("enabled")]
        public bool Enabled { get; init; }

        [JsonPropertyName("insertionOrder")]
        public int InsertionOrder { get; init; }

        [JsonPropertyName("useRegex")]
        public bool UseRegex { get; init; }

        [JsonPropertyName("constant")]
        public bool Constant { get; init; }

        // -- Fields that are not supported by all formats --
        [JsonPropertyName("depth")]
        public int Depth { get; set; }

        [JsonPropertyName("caseSensitive")]
        public bool CaseSensitive { get; set; }

        [JsonPropertyName("comment")]
        public string Comment { get; set; }

        [JsonPropertyName("secondaryKeys")]
        public List<string> SecondaryKeys { get; init; } = [];

        [JsonPropertyName("vectorized")]
        public bool Vectorized { get; init; }// Whether the entry uses vector/semantic search for matching instead of exact keywords

        [JsonPropertyName("matchWholeWord")]
        public bool MatchWholeWord { get; init; }

        [JsonPropertyName("probability")]
        public int ProbabilityPercentage { get; init; }

        [JsonPropertyName("positionInPrompt")]
        public int PositionInPrompt { get; init; }

        [JsonPropertyName("stickyForNbMessages")]
        public int StickyForNbMessages { get; init; }

        [JsonPropertyName("cooldown")]
        public int Cooldown { get; init; }

        [JsonPropertyName("ignoreTokensBudget")]
        public bool IgnoreTokensBudget { get; init; }

        [JsonPropertyName("delay")]
        public int Delay { get; init; }

        [JsonPropertyName("excludeRecursion")]
        public bool ExcludeRecursion { get; init; }

        [JsonPropertyName("preventRecursion")]
        public bool PreventRecursion { get; init; }

        [JsonPropertyName("onlyTriggeredByRecursion")]
        public bool OnlyTriggeredByRecursion { get; init; }

        [JsonPropertyName("tags")]
        public string[] Tags { get; init; }

        // -- Totally custom fields, owned by CohesiveRP --
        [JsonPropertyName("name")]
        public string Name { get; init; } = string.Empty;

        [JsonPropertyName("entryId")]
        public string EntryId { get; init; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonPropertyName("selectiveLogicBetweenKeysAndSecondaryKeys")]
        public KeysEvaluationLogicGate SelectiveLogicBetweenKeysAndSecondaryKeys { get; set; }
    }
}
