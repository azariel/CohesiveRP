using System.Text.Json.Serialization;
using CohesiveRP.Storage.DataAccessLayer.Pathfinder.ChatCharactersRolls.BusinessObjects;

namespace CohesiveRP.Storage.DataAccessLayer.Pathfinder.CharacterSheetInstances.BusinessObjects
{
    public class CharacterSheetInstance
    {
        [JsonPropertyName("characterSheetInstanceId")]
        public string CharacterSheetInstanceId { get; set; }

        [JsonPropertyName("characterSheetId")]
        public string CharacterSheetId { get; set; }

        [JsonPropertyName("characterId")]
        public string CharacterId { get; set; }

        [JsonPropertyName("personaId")]
        public string PersonaId { get; set; }

        [JsonPropertyName("characterSheet")]
        public CharacterSheet CharacterSheet { get; set; }

        [JsonPropertyName("isDirty")]
        public bool IsDirty { get; set; }

        [JsonPropertyName("consecutiveMessagesInScene")]
        public int ConsecutiveMessagesInScene { get; set; }

        [JsonPropertyName("consecutiveMessagesAbsentFromScene")]
        public int ConsecutiveMessagesAbsentFromScene { get; set; }// grace counter used ONLY to debounce sceneTracker flicker; always 0 except mid-departure-confirmation

        [JsonPropertyName("lastConfirmedAbsentMessageId")]
        public string LastConfirmedAbsentMessageId { get; set; }// messageId of the most recent cycle this character was confirmed absent; marks the start of their current/next presence session

        [JsonPropertyName("lastStatusCheckMessageId")]
        public string LastStatusCheckMessageId { get; set; }
    }
}