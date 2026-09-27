using System.Text.Json.Serialization;
using CohesiveRP.Common.WebApi;

namespace CohesiveRP.Core.WebApi.RequestDtos.Chat
{
    public class AddNewLorebookRequestDto : IWebApiRequestDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}
