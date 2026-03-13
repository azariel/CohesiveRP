using System.Text.Json.Serialization;
using CohesiveRP.Common.WebApi;

namespace CohesiveRP.Core.WebApi.RequestDtos.Characters
{
    public class ImportNewCharacterRequestDto : IWebApiRequestDto
    {
        [JsonPropertyName("file")]
        public IFormFile File { get; set; }
    }
}
