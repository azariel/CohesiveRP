using System.Text.Json.Serialization;

namespace CohesiveRP.Core.CharacterCards.Loaders.CCv3.BusinessObjects
{
    public record CCv3Asset
    {
        [JsonPropertyName("type")]
        public string Type { get; init; } = string.Empty;// "icon", "background", etc.

        [JsonPropertyName("uri")]
        public string Uri { get; init; } = string.Empty;// base64 data URL, HTTPS, or "embedded://"

        [JsonPropertyName("name")]
        public string Name { get; init; } = string.Empty;

        [JsonPropertyName("ext")]
        public string Ext { get; init; } = string.Empty;// file extension
    }
}
