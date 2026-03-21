using CohesiveRP.Common.WebApi;
using CohesiveRP.Common.Workflows;
using CohesiveRP.Core.WebApi.RequestDtos.Personas;

namespace CohesiveRP.Core.WebApi.Workflows.Personas.Abstractions
{
    public interface IImportAndReplacePersonaAvatarWorkflow : IWorkflow
    {
        Task<IWebApiResponseDto> ImportAvatarAsync(ImportAndReplacePersonaAvatarRequestDto requestDto);
    }
}
