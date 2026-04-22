using CohesiveRP.Common.Utils;
using CohesiveRP.Storage.DataAccessLayer.Characters.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.Chats;
using CohesiveRP.Storage.DataAccessLayer.SceneTracker.BusinessObjects;

namespace CohesiveRP.Core.Utils.Characters
{
    public static class CharacterAvatarsUtils
    {
        public static List<CharacterAvatar> GetCharacterSourceAvatars(CharacterDbModel characterDbModel, ClothingStateOfDress outfit)
        {
            List<CharacterAvatar> avatars = new();

            // Scan the Webapp public assets folder to find all the avatar images for the character
            string characterAvatarFolderPath = Path.Combine(WebConstants.CharactersAvatarFilePath, characterDbModel.Name.ToLowerInvariant(), WebConstants.SourceAvatarFolder, outfit.ToString().ToLowerInvariant());
            if (!Directory.Exists(characterAvatarFolderPath))
            {
                CharacterUtils.CreateCharacterAssets(characterDbModel);
                return avatars;
            }

            string[] avatarFiles = Directory.GetFiles(characterAvatarFolderPath, "*.*", SearchOption.AllDirectories);
            return avatarFiles
                .Where(file => file.EndsWith(".png", StringComparison.OrdinalIgnoreCase) || file.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) || file.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
                .Select(file => new CharacterAvatar
                {
                    AvatarFilePath = file.Replace(WebConstants.WebAppPublicFolder, ""),
                    AvatarFileName = Path.GetFileName(file),
                    AvatarSeed = AvatarUtils.GetSeedFromFileName(file),
                })
                .ToList();
        }

        public static bool DeleteCharacterAvatar(string characterName, string avatarFileName)
        {
            string characterFolderPath = Path.Combine(WebConstants.CharactersAvatarFilePath, characterName.ToLowerInvariant());

            if (!Directory.Exists(characterFolderPath))
                return false;

            var files = Directory.EnumerateFiles(characterFolderPath, "*.*", SearchOption.AllDirectories);
            var avatarsFilePath = files.Where(file => Path.GetFileName(file).Equals(avatarFileName, StringComparison.OrdinalIgnoreCase)).ToArray();
            if (avatarsFilePath != null && avatarsFilePath.Length > 0)
            {
                string lowerInvariantTarget = avatarFileName.ToLowerInvariant();
                foreach (var avatarFilePath in avatarsFilePath)
                {
                    if (Path.GetFileName(avatarFilePath).ToLowerInvariant() == lowerInvariantTarget)
                    {
                        File.Delete(avatarFilePath);
                    }
                }
                return true;
            }

            return false;
        }
    }
}
