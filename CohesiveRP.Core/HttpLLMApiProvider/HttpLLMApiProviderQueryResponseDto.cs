using System.Net;
using CohesiveRP.Common.WebApi;

namespace CohesiveRP.Core.HttpLLMApiProvider
{
    public class HttpLLMApiProviderQueryResponseDto : IWebApiResponseDto
    {
        public HttpStatusCode HttpResultCode { get; set; }
    }
}
