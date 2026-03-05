using CohesiveRP.Core.Services;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Format;

namespace CohesiveRP.Core.PromptContext.Builders.Directive
{
    public class PromptContextLastUserMessageBuilder : IPromptContextElementBuilder
    {
        public PromptContextLastUserMessageBuilder(IStorageService storageService)
        {
            
        }

        public async Task<string> BuildAsync(PromptContextFormatElement promptContextFormatElement)
        {
            string lastPlayerMessageContent = "";//TODO: fetch from Db
            string userPersonaName = "Potato";

            if(string.IsNullOrWhiteSpace(lastPlayerMessageContent))
            {
                return string.Empty;
            }

            return $"# Last message by {userPersonaName}{Environment.NewLine}{promptContextFormatElement?.Options?.Format?.Replace("{{item_description}}", lastPlayerMessageContent)}";
        }
    }
}
