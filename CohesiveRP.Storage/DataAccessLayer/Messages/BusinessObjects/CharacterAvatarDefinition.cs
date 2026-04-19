using System.Text.Json.Serialization;

namespace CohesiveRP.Storage.DataAccessLayer.Messages.BusinessObjects
{
    public class CharacterAvatarDefinition
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("outfit")]
        public string Outfit { get; set; }

        [JsonPropertyName("expression")]
        public string Expression { get; set; }

        [JsonPropertyName("filePath")]
        public string FilePath { get; set; }
    }
}
