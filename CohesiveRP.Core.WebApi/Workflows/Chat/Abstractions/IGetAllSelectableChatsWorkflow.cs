using CohesiveRP.Common.WebApi;

namespace CohesiveRP.Core.WebApi.Workflows.Chat.Abstractions
{
    public interface IGetAllSelectableChatsWorkflow
    {
        Task<IWebApiResponseDto> GetAllSelectableChats();
    }
}
