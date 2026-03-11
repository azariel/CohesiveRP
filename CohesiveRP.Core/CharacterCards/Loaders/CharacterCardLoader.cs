using CohesiveRP.Core.CharacterCards.Loaders.CCv3;
using SixLabors.ImageSharp;

namespace CohesiveRP.Core.CharacterCards.Loaders
{
    public static class CharacterCardLoader
    {
        public static ICharacterCard LoadCharacterCard(Image image)
        {
            // Try to load it as CCv3
            ICharacterCard characterCard = null;

            characterCard = CCv3CharacterCardLoader.TryLoadCharacterCard(image);

            // TODO: try other formats

            return characterCard;
        }
    }
}
