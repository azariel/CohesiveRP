using CohesiveRP.Core.CharacterCards.Loaders.CCv3;
using CohesiveRP.Core.Lorebooks;
using SixLabors.ImageSharp;

namespace CohesiveRP.Core.CharacterCards.Loaders
{
    public static class LorebookLoader
    {
        public static ILorebook LoadLoreBook(Image image)
        {
            // Try to load it as ccv3 compatible lorebook
            ILorebook lorebook = null;

            lorebook = CCv3LorebookLoader.TryLoadLorebook(image);

            // TODO: try other formats

            return lorebook;
        }

        public static ILorebook LoadLoreBook(string fileContent)
        {
            // Try to load it as ccv3 compatible lorebook
            ILorebook lorebook = null;

            lorebook = CCv3LorebookLoader.TryLoadLorebook(fileContent);

            // TODO: try other formats

            return lorebook;
        }
    }
}
