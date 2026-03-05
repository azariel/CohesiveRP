using CohesiveRP.Core.Services;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Format;

namespace CohesiveRP.Core.PromptContext.Builders.Directive
{
    public class PromptContextWorldBuilder : IPromptContextElementBuilder
    {
        public PromptContextWorldBuilder(IStorageService storageService)
        {
            
        }

        public async Task<string> BuildAsync(PromptContextFormatElement promptContextFormatElement)
        {
            string worldContent = $"Infer the world from the roleplay context.";//TODO: fetch from Db
            string userPersonaName = "userName";// TODO: fetch from Db

            if (string.IsNullOrWhiteSpace(worldContent))
            {
                worldContent = $"Infer the relevant characters from the roleplay context.{Environment.NewLine}{Environment.NewLine}";
            }

            return $"# World{Environment.NewLine}{promptContextFormatElement?.Options?.Format?
                .Replace("{{item_description}}", worldContent)
                .Replace("{{user}}", userPersonaName)}";
        }
    }
}
