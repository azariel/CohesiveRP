using CohesiveRP.Common.WebApi;
using Microsoft.AspNetCore.Mvc;

namespace CohesiveRP.Core.WebApi.RequestDtos.Chat
{
    public class DeleteChatCompletionPresetRequestDto : IWebApiRequestDto
    {
        [FromRoute]
        public string ChatCompletionPresetId { get; set; }
    }
}
