using System.Text.Json.Serialization;
using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.WebApi.RequestDtos.ChatCompletionPresets.BusinessObjects;
using Microsoft.AspNetCore.Mvc;

namespace CohesiveRP.Core.WebApi.RequestDtos.Chat
{
    public class UpdateChatCompletionPresetRequestDto : IWebApiRequestDto
    {
        [FromRoute]
        [JsonPropertyName("chatCompletionPresetId")]
        public string ChatCompletionPresetId { get; set; }

        [FromBody]
        public ChatCompletionPresetRequest ChatCompletionPreset { get; set; }
    }
}
