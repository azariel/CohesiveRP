using CohesiveRP.Storage.DataAccessLayer.Pathfinder.CharacterSheetInstances.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.Pathfinder.ChatCharactersRolls.BusinessObjects;

namespace CohesiveRP.Storage.Utils.Characters
{
    public static class CharacterSheetInstanceHelper
    {
        public static CharacterSheetInstance FindCharacterSheetInstanceFromCharacterName(List<CharacterSheetInstance> characterSheetInstances, string characterName)
        {
            string characterNameLower = characterName.ToLowerInvariant().Trim();
            var selectedCharacterSheetInstance = characterSheetInstances?.FirstOrDefault(f =>
            f.CharacterSheet.FirstName?.ToLowerInvariant().Trim() == characterNameLower ||
            f.CharacterSheet.LastName?.ToLowerInvariant().Trim() == characterNameLower ||
            $"{f.CharacterSheet.FirstName?.ToLowerInvariant().Trim()} {f.CharacterSheet.LastName?.ToLowerInvariant().Trim()}" == characterNameLower);

            return selectedCharacterSheetInstance;
        }

        public static bool IsCharacterSheetInScene(CharacterSheet characterSheet, string[] charactersInScene)
        {
            if(charactersInScene == null || charactersInScene.Length <= 0)
            {
                return false;
            }

            foreach (var characterInScene in charactersInScene)
            {
                string lowerInvariantCharacterNameInScene = characterInScene.ToLowerInvariant().Trim();
                if(characterSheet.FirstName?.ToLowerInvariant().Trim() == lowerInvariantCharacterNameInScene ||
                   characterSheet.LastName?.ToLowerInvariant().Trim() == lowerInvariantCharacterNameInScene ||
                   $"{characterSheet.FirstName?.ToLowerInvariant().Trim()} {characterSheet.LastName?.ToLowerInvariant().Trim()}" == lowerInvariantCharacterNameInScene)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
