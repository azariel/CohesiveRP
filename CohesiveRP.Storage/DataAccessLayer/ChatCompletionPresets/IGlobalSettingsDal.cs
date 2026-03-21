using CohesiveRP.Storage.DataAccessLayer.AIQueries;

namespace CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets
{
    public interface IChatCompletionPresetsDal
    {
        Task<ChatCompletionPresetsDbModel> GetChatCompletionPresetsAsync(string chatCompletionPresetId);
        Task<ChatCompletionPresetsDbModel[]> GetChatCompletionPresetsAsync();
    }
}
