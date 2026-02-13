namespace CohesiveRP.Storage.RequestDtos
{
    public class CreateUserRequestDto // : IRequestDto
    {

        // ********************************************************************
        //                            Properties
        // ********************************************************************
        public string UserName { get; set; }
        
        /// <summary>
        /// Optional
        /// </summary>
        public string Password { get; set; }
    }
}
