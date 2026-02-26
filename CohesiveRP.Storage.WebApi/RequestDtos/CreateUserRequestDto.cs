using CohesiveRP.Common.WebApi;

namespace CohesiveRP.Storage.WebApi.RequestDtos
{
    public class CreateUserRequestDto : IWebApiRequestDto
    {
        // ********************************************************************
        //                            Properties
        // ********************************************************************
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
