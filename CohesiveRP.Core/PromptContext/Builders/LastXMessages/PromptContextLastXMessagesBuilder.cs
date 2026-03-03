using CohesiveRP.Core.PromptContext.Format;
using CohesiveRP.Core.Services;

namespace CohesiveRP.Core.PromptContext.Builders.Directive
{
    public class PromptContextLastXMessagesBuilder : IPromptContextElementBuilder
    {
        public PromptContextLastXMessagesBuilder(IStorageService storageService)
        {
            
        }

        public async Task<string> BuildAsync(PromptContextFormatElement promptContextFormatElement)
        {
            string lastXMessagesContent = "";//TODO: fetch from Db

            if(string.IsNullOrWhiteSpace(lastXMessagesContent))
            {
                return string.Empty;
            }

            return $"# Last messages between you and the User{Environment.NewLine}{promptContextFormatElement?.Options?.Format?.Replace("{{item_description}}", lastXMessagesContent)}";
        }
    }
}
