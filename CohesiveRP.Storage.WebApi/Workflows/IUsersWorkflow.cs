using CohesiveRP.Common.WebApi;
using CohesiveRP.Common.Workflows;
using CohesiveRP.Storage.WebApi.RequestDtos;

namespace CohesiveRP.Storage.WebApi.Workflows
{
    public interface IUsersWorkflow : IWorkflow
    {
        Task<IWebApiReponseDto> CreateNewUser(CreateUserRequestDto userCreationRequestDto);
    }
}
