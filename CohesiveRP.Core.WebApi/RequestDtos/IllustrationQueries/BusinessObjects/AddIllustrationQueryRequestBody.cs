using System.Text.Json.Serialization;
using CohesiveRP.Storage.DataAccessLayer.InteractiveUserInputQueries.BusinessObjects;

namespace CohesiveRP.Core.WebApi.RequestDtos.IllustrationQueries.BusinessObjects
{
    public class AddIllustrationQueryRequestBody
    {
        [JsonPropertyName("chatId")]
        public string ChatId { get; set; }

        [JsonPropertyName("characterId")]
        public string CharacterId { get; set; }

        [JsonPropertyName("type")]
        public IllustratorQueryType Type { get; set; }
    }
}
