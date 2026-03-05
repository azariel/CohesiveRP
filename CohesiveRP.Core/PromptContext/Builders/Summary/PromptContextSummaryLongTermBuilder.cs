using CohesiveRP.Core.Services;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Format;

namespace CohesiveRP.Core.PromptContext.Builders.Directive
{
    public class PromptContextSummaryLongTermBuilder : IPromptContextElementBuilder
    {
        public PromptContextSummaryLongTermBuilder(IStorageService storageService)
        {
            
        }

        public async Task<string> BuildAsync(PromptContextFormatElement promptContextFormatElement)
        {
            string LongTermMemoryContent = "";//TODO: fetch from Db

            if(string.IsNullOrWhiteSpace(LongTermMemoryContent))
            {
                return string.Empty;
            }

            return $"# Long-term memory{Environment.NewLine}{promptContextFormatElement?.Options?.Format?.Replace("{{item_description}}", LongTermMemoryContent)}";
        }
    }
}
