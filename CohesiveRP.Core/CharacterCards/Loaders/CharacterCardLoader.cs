using CohesiveRP.Core.CharacterCards.Loaders.CCv3;
using SixLabors.ImageSharp;

namespace CohesiveRP.Core.CharacterCards.Loaders
{
    public static class CharacterCardLoader
    {
        public static ICharacterCard LoadCharacterCard(Image image)
        {
            ICharacterCard characterCard = null;

            // Try to load it as CohesiveRPv1. This is the format that embed the most information for CohesiveRP backend, so try it first
            characterCard = CohesiveRPv1CharacterCardLoader.TryLoadCharacterCard(image);

            if(characterCard != null)
                return characterCard;

            // Try to load a more standadized format, but embedding less information for CohesiveRP backend, but more for other backends, and with a better support in general

            // Try to load it as CCv3
            characterCard = CCv3CharacterCardLoader.TryLoadCharacterCard(image);
            if(characterCard != null)
                return characterCard;

            // Try to load it as Chara
            characterCard = CharaCharacterCardLoader.TryLoadCharacterCard(image);
            if(characterCard != null)
                return characterCard;

            return characterCard;
        }
    }
}
