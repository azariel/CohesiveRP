using CohesiveRP.Common.WebApi;
using CohesiveRP.Common.Workflows;
using CohesiveRP.Core.WebApi.RequestDtos.Characters;

namespace CohesiveRP.Core.WebApi.Workflows.Characters.Abstractions
{
    public interface IUpdateCharacterWorkflow : IWorkflow
    {
        Task<IWebApiResponseDto> UpdateCharacterAsync(UpdateCharacterRequestDto requestDto);
    }
}
