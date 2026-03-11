using CohesiveRP.Common.WebApi;
using CohesiveRP.Common.Workflows;

namespace CohesiveRP.Core.WebApi.Workflows.Characters.Abstractions
{
    public interface IGetAllCharactersWorkflow : IWorkflow
    {
        Task<IWebApiResponseDto> GetAllCharactersAsync();
    }
}
