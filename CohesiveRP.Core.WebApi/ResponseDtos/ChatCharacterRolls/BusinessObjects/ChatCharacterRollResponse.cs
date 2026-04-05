using System.Text.Json.Serialization;

namespace CohesiveRP.Core.WebApi.ResponseDtos.ChatCharacterRolls.BusinessObjects
{
    public class ChatCharacterRollResponse
    {
        [JsonPropertyName("characterId")]
        public string CharacterId { get; set; }

        [JsonPropertyName("characterName")]
        public string CharacterName { get; set; }

        [JsonPropertyName("rolls")]
        public List<ChatCharacterRoll> Rolls { get; set; }
    }
}
