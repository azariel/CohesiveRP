namespace CohesiveRP.Storage.Common
{
    public class StorageException : Exception
    {
        public StorageException(string errorCode, string message, Exception originalException = null) : base($"Code=[{errorCode}], Message=[{message}]", originalException)
        {
            this.ErrorCode = errorCode;
        }

        // ********************************************************************
        //                            Private
        // ********************************************************************
        string ErrorCode { get; set; }
    }
}
