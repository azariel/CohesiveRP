using CohesiveRP.Common.WebApi;
using CohesiveRP.Common.Workflows;

namespace CohesiveRP.Core.WebApi.Workflows.Personas.Abstractions
{
    public interface IGetPersonaByIdWorkflow : IWorkflow
    {
        Task<IWebApiResponseDto> GetPersonaByIdAsync(string personaId);
    }
}
