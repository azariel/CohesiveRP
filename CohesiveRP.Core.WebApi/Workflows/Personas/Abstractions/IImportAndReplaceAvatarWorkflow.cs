using CohesiveRP.Common.WebApi;
using CohesiveRP.Common.Workflows;
using CohesiveRP.Core.WebApi.RequestDtos.Personas;

namespace CohesiveRP.Core.WebApi.Workflows.Personas.Abstractions
{
    public interface IImportAndReplaceAvatarWorkflow : IWorkflow
    {
        Task<IWebApiResponseDto> ImportAvatarAsync(ImportAndReplaceAvatarRequestDto requestDto);
    }
}
