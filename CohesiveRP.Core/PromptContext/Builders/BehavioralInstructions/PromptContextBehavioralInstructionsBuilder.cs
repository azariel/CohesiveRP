using CohesiveRP.Core.Services;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Format;

namespace CohesiveRP.Core.PromptContext.Builders.Directive
{
    public class PromptContextBehavioralInstructionsBuilder : IPromptContextElementBuilder
    {
        public PromptContextBehavioralInstructionsBuilder(IStorageService storageService)
        {
            
        }

        public async Task<string> BuildAsync(PromptContextFormatElement promptContextFormatElement)
        {
            string behaviorInstructionsContent = "";//TODO: fetch from Db
            string userPersonaName = "userName";// TODO: fetch from Db

            if(string.IsNullOrWhiteSpace(behaviorInstructionsContent))
            {
                return string.Empty;
            }

            return $"# Your Behavioral Instruction{Environment.NewLine}{promptContextFormatElement?.Options?.Format?
                .Replace("{{item_description}}", behaviorInstructionsContent)
                .Replace("{{user}}", userPersonaName)}";
        }
    }
}
