using CohesiveRP.Storage.DataAccessLayer.AIQueries;

namespace CohesiveRP.Core.PromptContext.Abstractions
{
    public interface IPromptContextBuilder
    {
        Task<IPromptContext> BuildAsync(string chatId);
        ChatCompletionPresetsDbModel GetChatCompletionPreset();
    }
}
