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
                CharacterUtils.CreateCharacterAssets(characterDbModel.Name);
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

        public static void RefreshDefaultAvatars(string sourceCharacterFolder, bool forceUpdate = false)
        {
            // Set a main Avatar if none were already selected
            if (File.Exists($"{sourceCharacterFolder}\\{WebConstants.AvatarFileName}") && !forceUpdate)
            {
                return;

            }

            // Select the oldest file in the raws/clothed folder
            var clothedFolder = $"{sourceCharacterFolder}\\raws\\{ClothingStateOfDress.Clothed.ToString().ToLowerInvariant()}";
            var oldestFile = Directory.GetFiles(clothedFolder).OrderBy(f => File.GetCreationTimeUtc(f)).FirstOrDefault();
            if (oldestFile != null)
            {
                string outFilePath = $"{sourceCharacterFolder}\\{WebConstants.AvatarFileName}";
                if (File.Exists(outFilePath))
                    File.Delete(outFilePath);

                File.Copy(oldestFile, outFilePath);
            }

            // Set the main avatar for each outfit
            foreach (var outfit in Enum.GetValues(typeof(ClothingStateOfDress)))
            {
                string outfitDirectory = Path.Combine(sourceCharacterFolder, WebConstants.ExpressiveAvatarFolder, outfit.ToString().ToLowerInvariant());
                string rawOutfitDirectory = Path.Combine(sourceCharacterFolder, WebConstants.SourceAvatarFolder, outfit.ToString().ToLowerInvariant());

                if(!Directory.Exists(outfitDirectory) || !Directory.Exists(rawOutfitDirectory))
                    continue;

                string newOutfitAvatarCandidateFilePath = Directory.GetFiles(rawOutfitDirectory).OrderBy(f => File.GetCreationTimeUtc(f)).FirstOrDefault();

                if(string.IsNullOrWhiteSpace(newOutfitAvatarCandidateFilePath))
                    continue;

                string outFile = Path.Combine(outfitDirectory, WebConstants.AvatarFileName);

                if (File.Exists(outFile))
                    File.Delete(outFile);

                File.Copy(newOutfitAvatarCandidateFilePath, outFile);
            }
        }
    }
}
