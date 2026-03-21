using System.Text.Json;
using System.Text.Json.Serialization;

namespace CohesiveRP.Core.Lorebooks.Loaders.CCv3.BusinessObjects
{
    public class CCv3ChubExtension
{
    /// <summary>
    /// Empty object in the observed data; use a dictionary for forward compatibility.
    /// </summary>
    [JsonPropertyName("alt_expressions")]
    public Dictionary<string, JsonElement> AltExpressions { get; set; } = new();
 
    // Always null.
    [JsonPropertyName("expressions")]
    public JsonElement? Expressions { get; set; }
 
    [JsonPropertyName("full_path")]
    public string FullPath { get; set; } = string.Empty;
 
    [JsonPropertyName("id")]
    public int Id { get; set; }
 
    [JsonPropertyName("related_lorebooks")]
    public List<JsonElement> RelatedLorebooks { get; set; } = new();
}
}
