using System.Text.Json.Serialization;
using CohesiveRP.Common.Utils.BusinessObjects;

namespace CohesiveRP.Storage.DTOs.CharactersCohesionEnforcementResult
{
    public class CharactersCohesionEnforcementCharacterDecision
    {
        [JsonSchemaRequired]
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonSchemaRequired]
        [JsonPropertyName("why")]
        public string Why { get; set; }

        [JsonSchemaRequired]
        [JsonPropertyName("outcome")]
        public string Outcome { get; set; }

        [JsonSchemaRequired]
        [JsonPropertyName("sheet_detail")]
        public string SheetDetail { get; set; }

        [JsonSchemaRequired]
        [JsonPropertyName("concern_or_friction")]
        public string ConcernOrFriction { get; set; }

        [JsonSchemaRequired]
        [JsonPropertyName("attitude")]
        public string Attitude { get; set; }

        [JsonSchemaRequired]
        [JsonPropertyName("speech_intent")]
        public string SpeechIntent { get; set; }

        [JsonSchemaRequired]
        [JsonPropertyName("physical_constraints")]
        public string PhysicalConstraints { get; set; }
    }
}