using System.Text.Json.Serialization;
using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.WebApi.RequestDtos.IllustrationQueries.BusinessObjects;
using Microsoft.AspNetCore.Mvc;

namespace CohesiveRP.Core.WebApi.RequestDtos.IllustrationQueries
{
    public class AddIllustrationQueryRequestDto : IWebApiRequestDto
    {
        [FromBody]
        [JsonPropertyName("body")]
        public AddIllustrationQueryRequestBody Body { get; set; }
    }
}
