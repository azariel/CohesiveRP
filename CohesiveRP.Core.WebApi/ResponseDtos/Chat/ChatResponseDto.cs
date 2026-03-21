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
        public List<string> CharacterIds { get; internal set; }
        public List<string> LorebookIds { get; internal set; }
    }
}
