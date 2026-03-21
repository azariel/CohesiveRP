using System.Text.Json.Serialization;
using CohesiveRP.Common.WebApi;

namespace CohesiveRP.Core.WebApi.RequestDtos.Chat
{
    public class UpdateChatRequestDto : IWebApiRequestDto
    {
        [JsonPropertyName("chatId")]
        public string ChatId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("characterIds")]
        public List<string> CharacterIds { get; set; }

        [JsonPropertyName("lorebookIds")]
        public List<string> LorebookIds { get; set; }

        [JsonPropertyName("personaId")]
        public string PersonaId { get; set; }
    }
}
