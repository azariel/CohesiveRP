using CohesiveRP.Storage.DataAccessLayer.Chats;
using CohesiveRP.Storage.DataAccessLayer.SceneTracker.BusinessObjects;

namespace CohesiveRP.Core.Utils.Characters
{
    public static class CharacterUtils
    {
        private static void CreateRawsFolders(string directoryCharacter)
        {
            // Create the raws folder
            string rawsFolder = Path.Combine(directoryCharacter, WebConstants.SourceAvatarFolder);
            if (!Directory.Exists(rawsFolder))
            {
                Directory.CreateDirectory(rawsFolder);
            }

            // Create the raws/clothed folder
            string rawsClothedFolder = Path.Combine(rawsFolder, WebConstants.SourceAvatarClothedFolder);
            if (!Directory.Exists(rawsClothedFolder))
            {
                Directory.CreateDirectory(rawsClothedFolder);
            }

            // Create the raws/underwear folder
            string rawsUnderwearFolder = Path.Combine(rawsFolder, WebConstants.SourceAvatarUnderwearFolder);
            if (!Directory.Exists(rawsUnderwearFolder))
            {
                Directory.CreateDirectory(rawsUnderwearFolder);
            }

            // Create the raws/naked folder
            string rawsNakedFolder = Path.Combine(rawsFolder, WebConstants.SourceAvatarNakedFolder);
            if (!Directory.Exists(rawsNakedFolder))
            {
                Directory.CreateDirectory(rawsNakedFolder);
            }
        }

        private static void CreateExpressionFolders(string directoryCharacter)
        {
            // Create the expressions folder
            string expressionsFolder = Path.Combine(directoryCharacter, WebConstants.ExpressiveAvatarFolder);
            if (!Directory.Exists(expressionsFolder))
            {
                Directory.CreateDirectory(expressionsFolder);
            }

            // Create the expressions/clothed folder
            string expressionsClothedFolder = Path.Combine(expressionsFolder, WebConstants.ExpressiveAvatarClothedFolder);
            if (!Directory.Exists(expressionsClothedFolder))
            {
                Directory.CreateDirectory(expressionsClothedFolder);
            }

            // Create the expressions/underwear folder
            string expressionsUnderwearFolder = Path.Combine(expressionsFolder, WebConstants.ExpressiveAvatarUnderwearFolder);
            if (!Directory.Exists(expressionsUnderwearFolder))
            {
                Directory.CreateDirectory(expressionsUnderwearFolder);
            }

            // Create the expressions/naked folder
            string expressionsNakedFolder = Path.Combine(expressionsFolder, WebConstants.ExpressiveAvatarNakedFolder);
            if (!Directory.Exists(expressionsNakedFolder))
            {
                Directory.CreateDirectory(expressionsNakedFolder);
            }

            // Create the folders for every expressions
            CreateExpressionsSubFolders(expressionsClothedFolder);
            CreateExpressionsSubFolders(expressionsUnderwearFolder);
            CreateExpressionsSubFolders(expressionsNakedFolder);
        }

        private static void CreateExpressionsSubFolders(string sourceFolder)
        {
            MappedFacialExpression[] allHandledExpressions = Enum.GetValues<MappedFacialExpression>();

            foreach (var expression in allHandledExpressions)
            {
                string folderPath = Path.Combine(sourceFolder, expression.ToString().ToLowerInvariant());
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
            }
        }

        public static void CreateCharacterAssets(string characterName)
        {
            if (string.IsNullOrWhiteSpace(characterName))
                return;

            string directoryCharacter = Path.Combine(WebConstants.CharactersAvatarFilePath, characterName.ToLowerInvariant().Trim());
            if (!Directory.Exists(directoryCharacter))
            {
                Directory.CreateDirectory(directoryCharacter);
            }

            CreateRawsFolders(directoryCharacter);
            CreateExpressionFolders(directoryCharacter);
        }

        public static void MovePublicAssetsFolder(string oldCharacterName, string newCharacterName)
        {
            string oldDirectoryCharacter = Path.Combine(WebConstants.CharactersAvatarFilePath, oldCharacterName.ToLowerInvariant().Trim());
            string newDirectoryCharacter = Path.Combine(WebConstants.CharactersAvatarFilePath, newCharacterName.ToLowerInvariant().Trim());

            CreateCharacterAssets(newCharacterName);

            if (Directory.Exists(oldDirectoryCharacter))
            {
                string[] files = Directory.EnumerateFiles(oldDirectoryCharacter, "*.*", SearchOption.AllDirectories).ToArray();
                foreach (var filePath in files)
                {
                    string outFilePath = filePath.Replace(oldCharacterName, newCharacterName, StringComparison.InvariantCultureIgnoreCase);

                    try
                    {
                        string outDir = Path.GetDirectoryName(outFilePath);
                        if (!Directory.Exists(outDir))
                        {
                            Directory.CreateDirectory(outDir);
                        }

                        File.Copy(filePath, outFilePath, true);
                    } catch (Exception)
                    {
                        continue;
                    }

                    try
                    {
                        File.Delete(filePath);
                    } catch (Exception) { }
                }

                // Try to remove folder
                try
                {
                    Directory.Delete(oldDirectoryCharacter, true);
                } catch (Exception) { }
            }
        }
    }
}
