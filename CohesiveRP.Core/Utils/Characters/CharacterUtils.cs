using CohesiveRP.Core.Services;
using CohesiveRP.Storage.DataAccessLayer.Chats;
using CohesiveRP.Storage.DataAccessLayer.Pathfinder.ChatCharactersRolls.BusinessObjects;
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

        public static string GetFullPersonaNameFromContext(string personaId, PersonaDbModel personaLinkedToChat, CharacterSheetInstancesDbModel characterSheetInstancesTiedToChat, CharacterSheetDbModel personaCharacterSheetBlueprint)
        {
            if(string.IsNullOrWhiteSpace(personaId))    
            {
                return string.Empty;
            }

            // Get the user's name and character's name
            var personaName = personaLinkedToChat?.Name;// default is the 'name' of the persona
            var personaCharacterSheetInstance = characterSheetInstancesTiedToChat?.CharacterSheetInstances?.FirstOrDefault(f => f.PersonaId == personaId);

            // If that persona has a characterSheetInstance, use it
            if (personaCharacterSheetInstance?.CharacterSheet != null && !string.IsNullOrWhiteSpace(personaCharacterSheetInstance.CharacterSheet.FirstName))
            {
                personaName = personaCharacterSheetInstance.CharacterSheet.ComposeCharacterFullName();
            } else
            {
                // If the characterSheetInstance is a dummy one, fallback to the actual characterSheet if it's relevant
                if (personaCharacterSheetBlueprint?.CharacterSheet != null && !string.IsNullOrWhiteSpace(personaCharacterSheetBlueprint.CharacterSheet.FirstName))
                {
                    personaName = personaCharacterSheetBlueprint.CharacterSheet.ComposeCharacterFullName();
                }
            }

            return personaName;
        }

        public static string GetFullCharacterNameFromContext(string mainCharacterIdInChat, CharacterDbModel mainCharacterLinkedToChat, CharacterSheetInstancesDbModel characterSheetInstancesTiedToChat, CharacterSheetDbModel characterSheetBlueprint)
        {
            if(string.IsNullOrWhiteSpace(mainCharacterIdInChat))
            {
                return string.Empty;
            }

            // Get the user's name and character's name
            var characterName = mainCharacterLinkedToChat?.Name;// default is the 'name' of the character
            var characterCharacterSheetInstance = characterSheetInstancesTiedToChat?.CharacterSheetInstances?.FirstOrDefault(f => f.CharacterId == mainCharacterIdInChat);

            // If that character has a characterSheetInstance, use it
            if (characterCharacterSheetInstance?.CharacterSheet != null && !string.IsNullOrWhiteSpace(characterCharacterSheetInstance.CharacterSheet.FirstName))
            {
                characterName = characterCharacterSheetInstance.CharacterSheet.ComposeCharacterFullName();
            } else
            {
                // If the characterSheetInstance is a dummy one, fallback to the actual characterSheet if it's relevant
                if (characterSheetBlueprint?.CharacterSheet != null && !string.IsNullOrWhiteSpace(characterSheetBlueprint.CharacterSheet.FirstName))
                {
                    characterName = characterSheetBlueprint.CharacterSheet.ComposeCharacterFullName();
                }
            }

            return characterName;
        }

        public static string ComposeCharacterFullName(this CharacterSheet characterSheet)
        {
            if (characterSheet == null)
            {
                return string.Empty;
            }

            if (string.IsNullOrWhiteSpace(characterSheet.LastName))
            {
                return characterSheet.FirstName;
            }

            return $"{characterSheet.FirstName} {characterSheet.LastName}";
        }
    }
}
