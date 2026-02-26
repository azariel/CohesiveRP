namespace CohesiveRP.Storage.DataAccessLayer.Users.Requests
{
    public class CreateDbUserRequest
    {
        // ********************************************************************
        //                            Properties
        // ********************************************************************
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
