using System.Text;
using System.Text.Json;
using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.Serialization;
using CohesiveRP.Core.CharacterCards.Loaders.CCv3.BusinessObjects;
using CohesiveRP.Core.CharacterCards.Loaders.CohesiveRPv1.BusinessObjects;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Png.Chunks;
using Image = SixLabors.ImageSharp.Image;

namespace CohesiveRP.Core.CharacterCards.Loaders.CCv3
{
    public static class CohesiveRPv1CharacterCardLoader
    {
        private static string Decode(string base64) => Encoding.UTF8.GetString(Convert.FromBase64String(base64));
        private static string Encode(string rawString) => Convert.ToBase64String(Encoding.UTF8.GetBytes(rawString));

        public static ICharacterCard TryLoadCharacterCard(Image image)
        {
            if (image == null)
            {
                return null;
            }

            ICharacterCard card = null;
            card = TryLoadFormat(image);
            return card;
        }

        private static ICharacterCard TryLoadFormat(Image image)
        {
            try
            {
                PngMetadata pngMeta = image.Metadata.GetFormatMetadata(PngFormat.Instance);
                PngTextData? chunk = pngMeta.TextData.FirstOrDefault(f => f.Keyword.Equals("cohesiverp-v1", StringComparison.OrdinalIgnoreCase));

                if (chunk == null || !chunk.HasValue || chunk.Value.Value == null)
                    return null;

                var json = Decode(chunk.Value.Value);
                return JsonSerializer.Deserialize<CohesiveRPv1CharacterCard>(json);
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("9da64cb6-dc1e-4a2c-b8d5-ec976b56c2bb", $"Something went wrong when reading CohesiveRPv1 image.", ex);
                return null;
            }
        }

        /// <summary>
        /// Smash as much information as possible in all supported format inside the image.
        /// </summary>
        public static Image TryWriteFormat(Image image, CohesiveRPv1CharacterCard characterCardCRPv1, CCv3CharacterCard characterCardCCv3 = null)
        {
            try
            {
                PngMetadata pngMeta = image.Metadata.GetFormatMetadata(PngFormat.Instance);
                PngTextData? chunk = pngMeta.TextData.FirstOrDefault(f => f.Keyword.Equals("cohesiverp-v1", StringComparison.OrdinalIgnoreCase));
                string jsonToWrite = JsonCommonSerializer.SerializeToString(characterCardCRPv1);
                string encodedContent = Encode(jsonToWrite);

                if (chunk != null || chunk.HasValue || chunk.Value.Value != null)
                    pngMeta.TextData.Remove(chunk.Value);

                pngMeta.TextData.Add(new PngTextData("cohesiverp-v1", encodedContent, null, null));

                // Add the CCv3 information as well for compatibility
                if (characterCardCCv3 != null)
                {
                    CCv3CharacterCardLoader.TryWriteFormat(image, characterCardCCv3);
                }

                return image;

            } catch (Exception ex)
            {
                LoggingManager.LogToFile("b628da75-5162-4787-b0d6-997e5e8e2b57", $"Something went wrong when reading CohesiveRPv1 image.", ex);
                return null;
            }
        }
    }
}
