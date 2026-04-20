using CohesiveRP.Common.WebApi;
using CohesiveRP.Common.Workflows;

namespace CohesiveRP.Core.WebApi.Workflows.Messages.Abstractions
{
    public interface IGetPromptByChatIdWorkflow: IWorkflow
    {
        Task<IWebApiResponseDto> GeneratePromptForChatId(string chatId, string tag);
    }
}
