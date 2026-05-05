using System.Net;
using System.Text.Json.Serialization;
using CohesiveRP.Common.WebApi;
using CohesiveRP.Core.LLMProviderProcessors.Illustrator.MainCharacterAvatar.BusinessObjects;

namespace CohesiveRP.Core.WebApi.ResponseDtos.IllustrationQueries
{
    public class GeneratePromptInjectionForMainCharacterAvatarResponseDto : IWebApiResponseDto
    {
        [JsonConverter(typeof(JsonNumberEnumConverter<HttpStatusCode>))]
        [JsonPropertyName("code")]
        public HttpStatusCode HttpResultCode { get; set; }

        [JsonPropertyName("promptInjections")]
        public List<IllustratorGenerationContent> PromptInjections { get; set; }
    }
}
