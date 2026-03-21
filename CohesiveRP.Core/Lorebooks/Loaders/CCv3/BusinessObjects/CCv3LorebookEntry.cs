using System.Text.Json;
using System.Text.Json.Serialization;

namespace CohesiveRP.Core.Lorebooks.Loaders.CCv3.BusinessObjects
{
    public class CCv3LorebookEntry
{
    [JsonPropertyName("uid")]
    public int Uid { get; set; }
 
    // "key" is the legacy field; "keys" is the canonical field.
    // Both appear in the data and may differ, so we keep both.
    [JsonPropertyName("key")]
    public List<string> Key { get; set; } = new();
 
    [JsonPropertyName("keys")]
    public List<string> Keys { get; set; } = new();
 
    [JsonPropertyName("keysecondary")]
    public List<string> KeySecondary { get; set; } = new();
 
    [JsonPropertyName("secondary_keys")]
    public List<string> SecondaryKeys { get; set; } = new();
 
    [JsonPropertyName("comment")]
    public string Comment { get; set; } = string.Empty;
 
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
 
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
 
    [JsonPropertyName("id")]
    public int Id { get; set; }
 
    [JsonPropertyName("insertion_order")]
    public int InsertionOrder { get; set; }
 
    [JsonPropertyName("priority")]
    public int Priority { get; set; }
 
    [JsonPropertyName("order")]
    public int Order { get; set; }
 
    [JsonPropertyName("position")]
    public int Position { get; set; }
 
    [JsonPropertyName("depth")]
    public int Depth { get; set; } = 4;
 
    [JsonPropertyName("displayIndex")]
    public int DisplayIndex { get; set; }
 
    [JsonPropertyName("probability")]
    public int Probability { get; set; } = 100;
 
    [JsonPropertyName("useProbability")]
    public bool UseProbability { get; set; }
 
    [JsonPropertyName("selective")]
    public bool Selective { get; set; }
 
    [JsonPropertyName("selectiveLogic")]
    public int SelectiveLogic { get; set; }
 
    [JsonPropertyName("constant")]
    public bool Constant { get; set; }
 
    [JsonPropertyName("vectorized")]
    public bool Vectorized { get; set; }
 
    [JsonPropertyName("addMemo")]
    public bool AddMemo { get; set; }
 
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }
 
    [JsonPropertyName("disable")]
    public bool Disable { get; set; }
 
    [JsonPropertyName("ignoreBudget")]
    public bool IgnoreBudget { get; set; }
 
    [JsonPropertyName("excludeRecursion")]
    public bool ExcludeRecursion { get; set; }
 
    [JsonPropertyName("preventRecursion")]
    public bool PreventRecursion { get; set; }
 
    [JsonPropertyName("delayUntilRecursion")]
    public bool DelayUntilRecursion { get; set; }
 
    [JsonPropertyName("matchPersonaDescription")]
    public bool MatchPersonaDescription { get; set; }
 
    [JsonPropertyName("matchCharacterDescription")]
    public bool MatchCharacterDescription { get; set; }
 
    [JsonPropertyName("matchCharacterPersonality")]
    public bool MatchCharacterPersonality { get; set; }
 
    [JsonPropertyName("matchCharacterDepthPrompt")]
    public bool MatchCharacterDepthPrompt { get; set; }
 
    [JsonPropertyName("matchScenario")]
    public bool MatchScenario { get; set; }
 
    [JsonPropertyName("matchCreatorNotes")]
    public bool MatchCreatorNotes { get; set; }
 
    // Nullable – some entries explicitly set these to null.
    [JsonPropertyName("caseSensitive")]
    public bool? CaseSensitive { get; set; }
 
    // Legacy snake_case duplicate of caseSensitive.
    [JsonPropertyName("case_sensitive")]
    public bool? CaseSensitiveLegacy { get; set; }
 
    [JsonPropertyName("matchWholeWords")]
    public bool? MatchWholeWords { get; set; }
 
    // scanDepth is null in every entry observed; keep as nullable int.
    [JsonPropertyName("scanDepth")]
    public int? ScanDepth { get; set; }
 
    // useGroupScoring is always null in this file; use JsonElement so
    // we don't throw if a future schema version puts a value here.
    [JsonPropertyName("useGroupScoring")]
    public JsonElement? UseGroupScoring { get; set; }
 
    // role is always null; same approach.
    [JsonPropertyName("role")]
    public JsonElement? Role { get; set; }
 
    [JsonPropertyName("sticky")]
    public int Sticky { get; set; }
 
    [JsonPropertyName("cooldown")]
    public int Cooldown { get; set; }
 
    [JsonPropertyName("delay")]
    public int Delay { get; set; }
 
    [JsonPropertyName("group")]
    public string Group { get; set; } = string.Empty;
 
    [JsonPropertyName("groupOverride")]
    public bool GroupOverride { get; set; }
 
    [JsonPropertyName("groupWeight")]
    public int GroupWeight { get; set; }
 
    [JsonPropertyName("automationId")]
    public string AutomationId { get; set; } = string.Empty;
 
    [JsonPropertyName("outletName")]
    public string OutletName { get; set; } = string.Empty;
 
    [JsonPropertyName("triggers")]
    public List<string> Triggers { get; set; } = new();
 
    [JsonPropertyName("extensions")]
    public CCv3EntryExtensions Extensions { get; set; } = new();
 
    /// <summary>
    /// Present on most entries; absent on a handful – keep nullable.
    /// </summary>
    [JsonPropertyName("characterFilter")]
    public CCv3CharacterFilter? CharacterFilter { get; set; }
}
}
