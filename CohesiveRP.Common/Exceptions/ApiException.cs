using System.Net;

namespace CohesiveRP.Common.Exceptions
{
    public class ApiException : Exception
    {
        public HttpStatusCode Code { get; set; }

        public ApiException(HttpStatusCode code, string message, Exception originalException = null) : base(message, originalException)
        {
            Code = code;
        }
    }
}
