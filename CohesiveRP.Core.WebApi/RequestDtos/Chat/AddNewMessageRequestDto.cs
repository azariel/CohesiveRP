using System.Text.Json.Serialization;
using CohesiveRP.Common.WebApi;
using Microsoft.AspNetCore.Mvc;

namespace CohesiveRP.Core.WebApi.RequestDtos.Chat
{
    public class AddNewMessageRequestDto : IWebApiRequestDto
    {
        [FromRoute]
        [JsonPropertyName("chatId")]
        public string ChatId { get; set; }

        [FromBody]
        [JsonPropertyName("message")]
        public MessageRequestDto Message { get; set; }
    }
}
