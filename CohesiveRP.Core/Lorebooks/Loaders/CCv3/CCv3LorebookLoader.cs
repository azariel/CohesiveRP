using System.Text.Json;
using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Core.CharacterCards.Loaders.CCv3.BusinessObjects;
using CohesiveRP.Core.Lorebooks;
using CohesiveRP.Core.Lorebooks.Loaders.CCv3.BusinessObjects;
using Image = SixLabors.ImageSharp.Image;

namespace CohesiveRP.Core.CharacterCards.Loaders.CCv3
{
    public static class CCv3LorebookLoader
    {
        public static ILorebook TryLoadLorebook(string fileContent)
        {
            if (string.IsNullOrWhiteSpace(fileContent))
            {
                return null;
            }

            return TryLoadCCv3Format(fileContent);
        }

        public static ILorebook TryLoadLorebook(Image image)
        {
            if (image == null)
            {
                return null;
            }

            return TryLoadCCv3Format(image);
        }

        private static ILorebook TryLoadCCv3Format(string jsonContent)
        {
            if(string.IsNullOrWhiteSpace(jsonContent))
            {
                return null;
            }

            try
            {
                return JsonSerializer.Deserialize<CCv3Lorebook>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("61e2ec71-269e-4209-a50c-757eefe0b570", $"Something went wrong when reading CCv3 lorebook.", ex);
                return null;
            }
        }

        private static ILorebook TryLoadCCv3Format(Image image)
        {
            var characterCard = CCv3CharacterCardLoader.TryLoadCharacterCard(image) as CCv3CharacterCard;
            return characterCard?.Data?.CharacterBook;
        }
    }
}
