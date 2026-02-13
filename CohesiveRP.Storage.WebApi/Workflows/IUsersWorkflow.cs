using CohesiveRP.Common.WebApi;
using CohesiveRP.Common.Workflows;
using CohesiveRP.Storage.WebApi.ResponseDtos;

namespace CohesiveRP.Storage.WebApi.Workflows
{
    public interface IUsersWorkflow : IWorkflow
    {
        Task<IWebApiReponseDto> CreateNewUser(UserCreationRequestDto userCreationRequestDto);
    }
}
