using System.Text.Json.Serialization;
using CohesiveRP.Storage.DataAccessLayer.Pathfinder.ChatCharactersRolls.BusinessObjects;

namespace CohesiveRP.Core.WebApi.ResponseDtos.ChatCharacterRolls.BusinessObjects
{
    public class ChatCharacterInSceneCounterRolls
    {
        [JsonPropertyName("characterId")]
        public string CharacterId { get; set; }

        [JsonPropertyName("characterName")]
        public string CharacterName { get; set; }

        [JsonPropertyName("characterInSceneCounterRoll")]
        public CharacterInSceneCounterRoll CharacterInSceneCounterRoll { get; set; }
    }
}
