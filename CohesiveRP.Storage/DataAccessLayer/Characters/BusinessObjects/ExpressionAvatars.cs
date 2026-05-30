using System.Text.Json.Serialization;
using CohesiveRP.Storage.DataAccessLayer.SceneTracker.BusinessObjects;

namespace CohesiveRP.Storage.DataAccessLayer.Characters.BusinessObjects
{
    public class ExpressionAvatars
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonPropertyName("expression")]
        public MappedFacialExpression Expression { get; set; }

        [JsonPropertyName("avatars")]
        public List<CharacterAvatar> Avatars { get; set; }
    }
}
