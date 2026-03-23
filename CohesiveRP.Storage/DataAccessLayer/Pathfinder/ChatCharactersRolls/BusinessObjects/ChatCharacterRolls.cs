using System.Text.Json.Serialization;

namespace CohesiveRP.Storage.DataAccessLayer.Pathfinder.ChatCharactersRolls.BusinessObjects
{
    public class ChatCharacterRolls
    {
        [JsonPropertyName("characterSheetInstanceId")]
        public string CharacterSheetInstanceId { get; set; }

        [JsonPropertyName("rolls")]
        public List<ChatCharacterRoll> Rolls { get; set; }
    }
}
