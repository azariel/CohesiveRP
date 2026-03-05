using CohesiveRP.Core.Services;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Format;

namespace CohesiveRP.Core.PromptContext.Builders.Directive
{
    public class PromptContextDirectiveBuilder : IPromptContextElementBuilder
    {
        public PromptContextDirectiveBuilder(IStorageService storageService)
        {
        }

        public async Task<string> BuildAsync(PromptContextFormatElement promptContextFormatElement)
        {
            string userPersonaName = "userName";// TODO: fetch from Db
            return $"# Directive{Environment.NewLine}{promptContextFormatElement?.Options?.Format?.Replace("{{user}}", userPersonaName)}";
        }
    }
}
