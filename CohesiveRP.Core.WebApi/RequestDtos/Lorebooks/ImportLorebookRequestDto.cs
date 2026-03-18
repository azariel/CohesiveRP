using System.Text.Json.Serialization;
using CohesiveRP.Common.WebApi;

namespace CohesiveRP.Core.WebApi.RequestDtos.Personas
{
    public class ImportLorebookRequestDto : IWebApiRequestDto
    {
        [JsonPropertyName("file")]
        public IFormFile File { get; set; }
    }
}
