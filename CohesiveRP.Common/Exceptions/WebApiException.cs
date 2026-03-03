using System.Net;
using System.Text.Json.Serialization;
using CohesiveRP.Common.WebApi;

namespace CohesiveRP.Common.Exceptions
{
    /// <summary>
    /// Model mapped to what the client is expecting from a server-side exception Http response.
    /// </summary>
    public class WebApiException : IWebApiResponseDto
    {
        // ********************************************************************
        //                            Properties
        // ********************************************************************
        [JsonPropertyName("code")]
        public HttpStatusCode HttpResultCode { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }
    }
}
