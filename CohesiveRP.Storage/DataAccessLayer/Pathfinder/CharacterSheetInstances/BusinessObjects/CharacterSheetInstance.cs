using System.Text.Json.Serialization;
using CohesiveRP.Storage.DataAccessLayer.Pathfinder.ChatCharactersRolls.BusinessObjects;

namespace CohesiveRP.Storage.DataAccessLayer.Pathfinder.CharacterSheetInstances.BusinessObjects
{
    public class CharacterSheetInstance
    {
        [JsonPropertyName("characterSheetInstanceId")]
        public string CharacterSheetInstanceId { get; set; }

        [JsonPropertyName("characterId")]
        public string CharacterId { get; set; }

        [JsonPropertyName("personaId")]
        public string PersonaId { get; set; }

        [JsonPropertyName("characterSheet")]
        public CharacterSheet CharacterSheet { get; set; }
    }
}
