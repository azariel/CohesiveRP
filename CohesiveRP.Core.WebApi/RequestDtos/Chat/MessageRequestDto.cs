using System.Text.Json.Serialization;
using CohesiveRP.Common.WebApi;

namespace CohesiveRP.Core.WebApi.RequestDtos.Chat
{
    public class MessageRequestDto : IWebApiRequestDto
    {
        [JsonPropertyName("content")]
        public string Content { get; set; }

        [JsonPropertyName("timestampUtc")]
        public string timestampUtc { get; set; }
    }
}
