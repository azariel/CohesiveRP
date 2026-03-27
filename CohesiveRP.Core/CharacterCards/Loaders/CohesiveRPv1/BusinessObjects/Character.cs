using System.Text.Json.Serialization;

namespace CohesiveRP.Core.CharacterCards.Loaders.CohesiveRPv1.BusinessObjects
{
    public class Character
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("creator")]
        public string Creator { get; set; }

        [JsonPropertyName("creatorNotes")]
        public string CreatorNotes { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; }

        [JsonPropertyName("firstMessage")]
        public string FirstMessage { get; set; }

        [JsonPropertyName("alternateGreetings")]
        public List<string> AlternateGreetings { get; set; }
    }
}
