using System.Text.Json.Serialization;
using CohesiveRP.Common.WebApi;

namespace CohesiveRP.Storage.WebApi.ResponseDtos
{
    public class GetChatResponseDto : IWebApiReponseDto
    {
        [JsonPropertyName("chatId")]
        public string ChatId { get; set; }
    }
}
