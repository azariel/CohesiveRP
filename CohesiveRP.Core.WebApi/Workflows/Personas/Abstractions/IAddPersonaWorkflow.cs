using CohesiveRP.Common.WebApi;
using CohesiveRP.Common.Workflows;
using CohesiveRP.Core.WebApi.RequestDtos.Chat;

namespace CohesiveRP.Core.WebApi.Workflows.Personas.Abstractions
{
    public interface IAddPersonaWorkflow : IWorkflow
    {
        Task<IWebApiResponseDto> AddPersonaAsync(AddNewPersonaRequestDto requestDto);
    }
}
