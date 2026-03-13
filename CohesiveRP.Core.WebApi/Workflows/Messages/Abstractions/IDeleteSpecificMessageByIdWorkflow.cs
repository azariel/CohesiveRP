using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.WebApi.RequestDtos.Chat;

namespace CohesiveRP.Core.WebApi.Workflows.Chat.Abstractions
{
    public interface IDeleteSpecificMessageByIdWorkflow
    {
        Task<IWebApiResponseDto> DeleteSpecificMessage(GetSpecificMessageRequestDto requestDto);
    }
}
