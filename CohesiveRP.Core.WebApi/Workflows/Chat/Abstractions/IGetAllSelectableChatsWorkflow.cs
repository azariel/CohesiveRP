using CohesiveRP.Common.WebApi;
using CohesiveRP.Common.Workflows;

namespace CohesiveRP.Core.WebApi.Workflows.Chat.Abstractions
{
    public interface IGetAllSelectableChatsWorkflow : IWorkflow
    {
        Task<IWebApiResponseDto> GetAllSelectableChats();
    }
}
