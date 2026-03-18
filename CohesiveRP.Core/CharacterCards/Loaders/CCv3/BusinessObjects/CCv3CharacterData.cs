using System.Text.Json.Serialization;

namespace CohesiveRP.Core.CharacterCards.Loaders.CCv3.BusinessObjects
{
    public record CCv3CharacterData
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; init; } = string.Empty;

    [JsonPropertyName("personality")]
    public string Personality { get; init; } = string.Empty;

    [JsonPropertyName("scenario")]
    public string Scenario { get; init; } = string.Empty;

    [JsonPropertyName("first_mes")]
    public string FirstMessage { get; init; } = string.Empty;

    [JsonPropertyName("mes_example")]
    public string MessageExample { get; init; } = string.Empty;

    [JsonPropertyName("creator_notes")]
    public string CreatorNotes { get; init; } = string.Empty;

    [JsonPropertyName("system_prompt")]
    public string SystemPrompt { get; init; } = string.Empty;

    [JsonPropertyName("post_history_instructions")]
    public string PostHistoryInstructions { get; init; } = string.Empty;

    [JsonPropertyName("alternate_greetings")]
    public List<string> AlternateGreetings { get; init; } = [];

    [JsonPropertyName("group_only_greetings")]
    public List<string> GroupOnlyGreetings { get; init; } = [];

    [JsonPropertyName("tags")]
    public List<string> Tags { get; init; } = [];

    [JsonPropertyName("creator")]
    public string Creator { get; init; } = string.Empty;

    [JsonPropertyName("character_version")]
    public string CharacterVersion { get; init; } = string.Empty;

    [JsonPropertyName("nickname")]
    public string? Nickname { get; init; }

    [JsonPropertyName("source")]
    public List<string>? Source { get; init; }

    [JsonPropertyName("creation_date")]
    public long? CreationDate { get; init; }

    [JsonPropertyName("modification_date")]
    public long? ModificationDate { get; init; }

    [JsonPropertyName("character_book")]
    public CCv3CharacterLorebook? CharacterBook { get; init; }

    [JsonPropertyName("assets")]
    public List<CCv3Asset>? Assets { get; init; }
}
}
