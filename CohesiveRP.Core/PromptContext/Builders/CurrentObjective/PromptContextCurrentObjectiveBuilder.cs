using CohesiveRP.Core.PromptContext.Format;
using CohesiveRP.Core.Services;

namespace CohesiveRP.Core.PromptContext.Builders.Directive
{
    public class PromptContextCurrentObjectiveBuilder : IPromptContextElementBuilder
    {
        public PromptContextCurrentObjectiveBuilder(IStorageService storageService)
        {
            
        }

        public async Task<string> BuildAsync(PromptContextFormatElement promptContextFormatElement)
        {
            string currentObjectiveContent = "";//TODO: fetch from Db

            if(string.IsNullOrWhiteSpace(currentObjectiveContent))
            {
                return string.Empty;
            }

            return $"# Current Objective (story progression objective){Environment.NewLine}{promptContextFormatElement?.Options?.Format?.Replace("{{item_description}}", currentObjectiveContent)}";
        }
    }
}
