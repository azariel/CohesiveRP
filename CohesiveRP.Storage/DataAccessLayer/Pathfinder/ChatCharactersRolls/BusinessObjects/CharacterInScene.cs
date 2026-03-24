using System.Text.Json.Serialization;

namespace CohesiveRP.Storage.DataAccessLayer.Pathfinder.ChatCharactersRolls.BusinessObjects
{
    public class CharacterInScene
    {
        [JsonPropertyName("characterSheetInstanceId")]
        public string CharacterSheetInstanceId { get; set; }

        [JsonPropertyName("characterInSceneCounterRoll")]
        public CharacterInSceneCounterRoll CharacterInSceneCounterRoll { get; set; }
    }
}
