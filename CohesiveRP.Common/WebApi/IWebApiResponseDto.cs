using System.Net;
using System.Text.Json.Serialization;

namespace CohesiveRP.Common.WebApi
{
    /// <summary>
    /// Every response from a WebApi endpoint must implement this interface.
    /// </summary>
    public interface IWebApiResponseDto
    {
        [JsonConverter(typeof(JsonNumberEnumConverter<HttpStatusCode>))]
        [JsonPropertyName("code")]
        public HttpStatusCode HttpResultCode { get; set; }
    }
}
