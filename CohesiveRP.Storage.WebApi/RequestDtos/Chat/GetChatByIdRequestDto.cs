using System.Text.Json.Serialization;
using CohesiveRP.Common.WebApi;
using Microsoft.AspNetCore.Mvc;

namespace CohesiveRP.Storage.WebApi.RequestDtos.Chat
{
    public class GetChatByIdRequestDto : IWebApiRequestDto
    {
        [FromRoute]
        [JsonPropertyName("chatId")]
        public string ChatId { get; set; }
    }
}
