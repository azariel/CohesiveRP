using CohesiveRP.Core.PromptContext.Format;
using CohesiveRP.Core.Services;

namespace CohesiveRP.Core.PromptContext.Builders.Directive
{
    public class PromptContextSceneTrackerBuilder : IPromptContextElementBuilder
    {
        public PromptContextSceneTrackerBuilder(IStorageService storageService)
        {
            
        }

        public async Task<string> BuildAsync(PromptContextFormatElement promptContextFormatElement)
        {
            string sceneTrackerContent = "";//TODO: fetch from Db

            if(string.IsNullOrWhiteSpace(sceneTrackerContent))
            {
                return string.Empty;
            }

            return $"# Scene Tracker (details on current scene){Environment.NewLine}{promptContextFormatElement?.Options?.Format?.Replace("{{item_description}}", sceneTrackerContent)}";
        }
    }
}
