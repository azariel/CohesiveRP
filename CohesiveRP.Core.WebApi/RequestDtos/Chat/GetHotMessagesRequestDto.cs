using System.Text.Json.Serialization;
using CohesiveRP.Common.WebApi;
using Microsoft.AspNetCore.Mvc;

namespace CohesiveRP.Core.WebApi.RequestDtos.Chat
{
    public class GetHotMessagesRequestDto : IWebApiRequestDto
    {
        [FromRoute]
        [JsonPropertyName("chatId")]
        public string ChatId { get; set; }
    }
}
