using System.Net;
using System.Text.Json.Serialization;
using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.WebApi.ResponseDtos.Chat.BusinessObjects;

namespace CohesiveRP.Core.WebApi.ResponseDtos.Chat
{
    public class ChatResponseDto : ChatDefinition, IWebApiResponseDto
    {
        [JsonConverter(typeof(JsonNumberEnumConverter<HttpStatusCode>))]
        [JsonPropertyName("code")]
        public HttpStatusCode HttpResultCode { get; set; }

        [JsonPropertyName("characterIds")]
        public List<string> CharacterIds { get; set; }

        [JsonPropertyName("lorebookIds")]
        public List<string> LorebookIds { get; set; }

        [JsonPropertyName("personaId")]
        public string PersonaId { get; set; }
    }
}
