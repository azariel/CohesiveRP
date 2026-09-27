using System.Text.Json;
using CohesiveRP.Core.PromptContext.Abstractions;

namespace CohesiveRP.Core.WebApi.Workflows.Characters.CharacterSheets
{
    public class CharacterSheetRegenerationPromptContext : IPromptContext
    {
        public string Value { get; set; }
        public IPromptMessage[] Messages { get; set; }
        public List<IShareableContextLink> ShareableContextLinks { get; set; }
        public int? MaxTokensToGenerate { get; set; }
        public string JsonSchemaName  { get; set; }
        public JsonElement? JsonSchemaDocument  { get; set; }
    }
}
