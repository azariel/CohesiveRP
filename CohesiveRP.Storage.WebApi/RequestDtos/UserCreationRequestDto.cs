using CohesiveRP.Common.WebApi;

namespace CohesiveRP.Storage.WebApi.ResponseDtos
{
    public class UserCreationRequestDto : IWebApiRequestDto
    {
        // ********************************************************************
        //                            Properties
        // ********************************************************************
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
