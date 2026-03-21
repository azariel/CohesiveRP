using System.Text.Json.Serialization;
using CohesiveRP.Common.WebApi;
using Microsoft.AspNetCore.Mvc;

namespace CohesiveRP.Core.WebApi.RequestDtos.Personas
{
    public class ImportAndReplaceLorebookAvatarRequestDto : IWebApiRequestDto
    {
        [FromRoute]
        public string LorebookId { get; set; }

        [JsonPropertyName("file")]
        public IFormFile File { get; set; }
    }
}
