using System.Text.Json.Serialization;
using CohesiveRP.Common.WebApi;

namespace CohesiveRP.Core.WebApi.RequestDtos.Chat
{
    public class AddNewChatRequestDto : IWebApiRequestDto
    {
        [JsonPropertyName("characterId")]
        public string CharacterId { get; set; }
    }
}
