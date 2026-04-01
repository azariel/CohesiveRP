namespace CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Format
{
    public class PromptContextFormatElementRelevantCharactersOptions : PromptContextFormatElementOptions
    {
        public bool IncludeKnownCharacters { get; set; } = true;
    }
}
