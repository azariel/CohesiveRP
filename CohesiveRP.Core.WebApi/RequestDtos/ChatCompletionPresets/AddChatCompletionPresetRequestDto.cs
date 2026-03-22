using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.WebApi.RequestDtos.ChatCompletionPresets.BusinessObjects;
using Microsoft.AspNetCore.Mvc;

namespace CohesiveRP.Core.WebApi.RequestDtos.Chat
{
    public class AddChatCompletionPresetRequestDto : IWebApiRequestDto
    {
        [FromBody]
        public ChatCompletionPresetRequest ChatCompletionRequest { get; set; }
    }
}
