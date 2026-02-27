using System.Text.Json.Serialization;
using CohesiveRP.Common.WebApi;

namespace CohesiveRP.Core.WebApi.ResponseDtos.Storage
{
    public class ChatResponseDto : IWebApiReponseDto
    {
        [JsonPropertyName("chatId")]
        public string ChatId { get; set; }
    }
}
