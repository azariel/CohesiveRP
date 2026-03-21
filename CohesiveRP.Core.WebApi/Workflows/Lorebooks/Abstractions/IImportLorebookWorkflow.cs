using CohesiveRP.Common.WebApi;
using CohesiveRP.Common.Workflows;
using CohesiveRP.Core.WebApi.RequestDtos.Personas;

namespace CohesiveRP.Core.WebApi.Workflows.Lorebooks.Abstractions
{
    public interface IImportLorebookWorkflow : IWorkflow
    {
        Task<IWebApiResponseDto> ImportAsync(ImportLorebookRequestDto requestDto);
    }
}
