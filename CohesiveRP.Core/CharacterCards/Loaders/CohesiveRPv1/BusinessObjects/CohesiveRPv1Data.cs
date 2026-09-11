using System.Text.Json.Serialization;
using CohesiveRP.Storage.DataAccessLayer.Pathfinder.ChatCharactersRolls.BusinessObjects;
using CohesiveRP.Storage.DTOs;

namespace CohesiveRP.Core.CharacterCards.Loaders.CohesiveRPv1.BusinessObjects
{
    public class CohesiveRPv1Data
    {
        [JsonPropertyName("character")]
        public Character Character { get; set; }

        [JsonPropertyName("characterSheet")]
        public CharacterSheet CharacterSheet { get; set; }
    }
}
