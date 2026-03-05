using CohesiveRP.Core.Services;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Format;

namespace CohesiveRP.Core.PromptContext.Builders.Directive
{
    public class PromptContextSummaryMediumTermBuilder : IPromptContextElementBuilder
    {
        public PromptContextSummaryMediumTermBuilder(IStorageService storageService)
        {
            
        }

        public async Task<string> BuildAsync(PromptContextFormatElement promptContextFormatElement)
        {
            string MediumTermMemoryContent = "";//TODO: fetch from Db

            if(string.IsNullOrWhiteSpace(MediumTermMemoryContent))
            {
                return string.Empty;
            }

            return $"# Medium-term memory{Environment.NewLine}{promptContextFormatElement?.Options?.Format?.Replace("{{item_description}}", MediumTermMemoryContent)}";
        }
    }
}
