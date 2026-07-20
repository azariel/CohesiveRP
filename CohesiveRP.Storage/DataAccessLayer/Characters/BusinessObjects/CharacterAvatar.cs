using System.Text.Json.Serialization;

namespace CohesiveRP.Storage.DataAccessLayer.Characters.BusinessObjects
{
    public class CharacterAvatar
    {
        [JsonPropertyName("avatarFilePath")]
        public string AvatarFilePath { get; set; }

        [JsonPropertyName("avatarFileName")]
        public string AvatarFileName { get; set; }

        [JsonPropertyName("avatarSeed")]
        public string AvatarSeed { get; set; }
    }
}
