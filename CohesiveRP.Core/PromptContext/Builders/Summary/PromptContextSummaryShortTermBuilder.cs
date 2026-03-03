using CohesiveRP.Core.PromptContext.Format;
using CohesiveRP.Core.Services;

namespace CohesiveRP.Core.PromptContext.Builders.Directive
{
    public class PromptContextSummaryShortTermBuilder : IPromptContextElementBuilder
    {
        public PromptContextSummaryShortTermBuilder(IStorageService storageService)
        {
            
        }

        public async Task<string> BuildAsync(PromptContextFormatElement promptContextFormatElement)
        {
            string ShortTermMemoryContent = "";//TODO: fetch from Db

            if(string.IsNullOrWhiteSpace(ShortTermMemoryContent))
            {
                return string.Empty;
            }

            return $"# Short-term memory{Environment.NewLine}{promptContextFormatElement?.Options?.Format?.Replace("{{item_description}}", ShortTermMemoryContent)}";
        }
    }
}
