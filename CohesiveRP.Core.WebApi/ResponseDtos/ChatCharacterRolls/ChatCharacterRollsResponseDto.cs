using System.Net;
using System.Text.Json.Serialization;
using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.WebApi.ResponseDtos.ChatCharacterRolls.BusinessObjects;

namespace CohesiveRP.Core.WebApi.ResponseDtos.ChatCompletionPresets
{
    public class ChatCharacterRollsResponseDto : IWebApiResponseDto
    {
        [JsonConverter(typeof(JsonNumberEnumConverter<HttpStatusCode>))]
        [JsonPropertyName("code")]
        public HttpStatusCode HttpResultCode { get; set; }

        [JsonPropertyName("rolls")]
        public ChatCharacterRollResponse[] Rolls { get; set; }
    }
}
