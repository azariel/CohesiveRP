using CohesiveRP.Core.PromptContext.Format;
using CohesiveRP.Core.Services;

namespace CohesiveRP.Core.PromptContext.Builders.Directive
{
    public class PromptContextSummaryExtraTermBuilder : IPromptContextElementBuilder
    {
        public PromptContextSummaryExtraTermBuilder(IStorageService storageService)
        {
            
        }

        public async Task<string> BuildAsync(PromptContextFormatElement promptContextFormatElement)
        {
            string ExtraMemoryContent = "";//TODO: fetch from Db

            if(string.IsNullOrWhiteSpace(ExtraMemoryContent))
            {
                return string.Empty;
            }

            return $"# Extra-Long-term memory{Environment.NewLine}{promptContextFormatElement?.Options?.Format?.Replace("{{item_description}}", ExtraMemoryContent)}";
        }
    }
}
