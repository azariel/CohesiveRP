using System.Text;
using System.Text.Json;
using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Core.CharacterCards.Loaders.CCv3.BusinessObjects;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Png.Chunks;

namespace CohesiveRP.Core.CharacterCards.Loaders.CCv3
{
    public static class CCv3CharacterCardLoader
    {
        private static string Decode(string base64) => Encoding.UTF8.GetString(Convert.FromBase64String(base64));

        public static ICharacterCard TryLoadCharacterCard(Image image)
        {
            if (image == null)
            {
                return null;
            }

            try
            {
                PngMetadata pngMeta = image.Metadata.GetFormatMetadata(PngFormat.Instance);
                PngTextData? ccv3Chunk = pngMeta.TextData.FirstOrDefault(f => f.Keyword.Equals("ccv3", StringComparison.OrdinalIgnoreCase));

                if (ccv3Chunk == null || !ccv3Chunk.HasValue)
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
    }
}
