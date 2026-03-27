using System.Text;
using System.Text.Json;
using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.Serialization;
using CohesiveRP.Core.CharacterCards.Loaders.CCv3.BusinessObjects;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Png.Chunks;
using Image = SixLabors.ImageSharp.Image;

namespace CohesiveRP.Core.CharacterCards.Loaders.CCv3
{
    public static class CCv3CharacterCardLoader
    {
        private static string Decode(string base64) => Encoding.UTF8.GetString(Convert.FromBase64String(base64));
        private static string Encode(string rawString) => Convert.ToBase64String(Encoding.UTF8.GetBytes(rawString));

        private static ICharacterCard TryLoadCCv3Format(Image image)
        {
            try
            {
                PngMetadata pngMeta = image.Metadata.GetFormatMetadata(PngFormat.Instance);
                PngTextData? ccv3Chunk = pngMeta.TextData.FirstOrDefault(f => f.Keyword.Equals("ccv3", StringComparison.OrdinalIgnoreCase) || f.Keyword.Equals("chara", StringComparison.OrdinalIgnoreCase));

                if (ccv3Chunk == null || !ccv3Chunk.HasValue || ccv3Chunk.Value.Value == null)
                    return null;

                var json = Decode(ccv3Chunk.Value.Value);
                return JsonSerializer.Deserialize<CCv3CharacterCard>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("59cfbe7d-faa2-44b5-8bd0-38ab738c991b", $"Something went wrong when reading CCv3 image.", ex);
                return null;
            }
        }

        public static ICharacterCard TryLoadCharacterCard(Image image)
        {
            if (image == null)
            {
                return null;
            }

            ICharacterCard card = null;
            card = TryLoadCCv3Format(image);

            //if (card == null)
            //{
            //    card = TryLoadCharaFormat(image);
            //}

            return card;
        }

        /// <summary>
        /// Smash as much information as possible in all supported format inside the image.
        /// </summary>
        public static Image TryWriteFormat(Image image, CCv3CharacterCard characterCardCCv3)
        {
            try
            {
                PngMetadata pngMeta = image.Metadata.GetFormatMetadata(PngFormat.Instance);
                PngTextData? chunk = pngMeta.TextData.FirstOrDefault(f => f.Keyword.Equals("ccv3", StringComparison.OrdinalIgnoreCase));
                string jsonToWrite = JsonCommonSerializer.SerializeToString(characterCardCCv3);
                string encodedContent = Encode(jsonToWrite);

                if (chunk != null || chunk.HasValue || chunk.Value.Value != null)
                    pngMeta.TextData.Remove(chunk.Value);

                pngMeta.TextData.Add(new PngTextData("ccv3", encodedContent, null, null));

                // Also remove the "chara" tag if any available since it duplicates the ccv3 one and take space for no reason (TODO: check if other software load that instead of ccv3?)
                PngTextData? charaChunk = pngMeta.TextData.FirstOrDefault(f => f.Keyword.Equals("chara", StringComparison.OrdinalIgnoreCase));
                if(charaChunk != null || charaChunk.HasValue || charaChunk.Value.Value != null)
                    pngMeta.TextData.Remove(charaChunk.Value);

                // Looks like its required for compatibility. I love duplicating information : /
                pngMeta.TextData.Add(new PngTextData("chara", encodedContent, null, null));

                return image;

            } catch (Exception ex)
            {
                LoggingManager.LogToFile("7358cd0d-9c66-49a0-af3b-81b632c8856f", $"Something went wrong when reading CohesiveRPv1 image.", ex);
                return null;
            }
        }
    }
}
