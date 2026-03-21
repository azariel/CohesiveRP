using System.Text.Json.Serialization;

namespace CohesiveRP.Core.Lorebooks.Loaders.CCv3.BusinessObjects
{
    public class CCv3LorebookExtensions
{
    [JsonPropertyName("chub")]
    public CCv3ChubExtension Chub { get; set; } = new();
}
}
