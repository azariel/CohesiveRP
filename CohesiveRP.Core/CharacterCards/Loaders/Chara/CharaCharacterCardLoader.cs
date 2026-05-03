using System.Text;
using System.Text.Json;
using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.Serialization;
using CohesiveRP.Common.Utils;
using CohesiveRP.Core.CharacterCards.Loaders.CCv3.BusinessObjects;
using CohesiveRP.Core.CharacterCards.Loaders.Chara.BusinessObjects;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Png.Chunks;
using SQLitePCL;
using Image = SixLabors.ImageSharp.Image;

namespace CohesiveRP.Core.CharacterCards.Loaders.CCv3
{
    /// <summary>
    /// JanitorAI style.
    /// </summary>
    public static class CharaCharacterCardLoader
    {
        private static string Decode(string base64) => Encoding.UTF8.GetString(Convert.FromBase64String(base64));
        private static string Encode(string rawString) => Convert.ToBase64String(Encoding.UTF8.GetBytes(rawString));

        private static ICharacterCard TryLoadCustomFormat(Image image)
        {
            try
            {
                PngMetadata pngMeta = image.Metadata.GetFormatMetadata(PngFormat.Instance);
                PngTextData? chunk = pngMeta.TextData.FirstOrDefault(f => f.Keyword.Equals("chara", StringComparison.OrdinalIgnoreCase));

                if (chunk == null || !chunk.HasValue || chunk.Value.Value == null)
                    return null;

                var json = Decode(chunk.Value.Value);
                var model = JsonSerializer.Deserialize<CharaCharacterCard>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = false
                });

                if(model == null || string.IsNullOrWhiteSpace(model.Name))
                    return null;

                // Sanitize fields
                model.Description = StringUtils.RemoveXmlTags(model.Description);
                model.Personality = StringUtils.RemoveXmlTags(model.Personality);

                return model;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("097d61e6-5be7-47b2-8ffb-e3a65f1b7fb8", $"Something went wrong when reading Chara image.", ex);
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
            card = TryLoadCustomFormat(image);
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
                PngTextData? chunk = pngMeta.TextData.FirstOrDefault(f => f.Keyword.Equals("chara", StringComparison.OrdinalIgnoreCase));
                string jsonToWrite = JsonCommonSerializer.SerializeToString(characterCardCCv3);
                string encodedContent = Encode(jsonToWrite);

                if (chunk != null || chunk.HasValue || chunk.Value.Value != null)
                    pngMeta.TextData.Remove(chunk.Value);

                pngMeta.TextData.Add(new PngTextData("chara", encodedContent, null, null));
                return image;

            } catch (Exception ex)
            {
                LoggingManager.LogToFile("0e58d598-2895-4c17-9932-94085fa880b4", $"Something went wrong when reading CohesiveRPv1 image.", ex);
                return null;
            }
        }
    }
}
