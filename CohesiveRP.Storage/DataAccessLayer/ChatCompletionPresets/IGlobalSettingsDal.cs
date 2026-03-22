using CohesiveRP.Storage.DataAccessLayer.AIQueries;

namespace CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets
{
    public interface IChatCompletionPresetsDal
    {
        Task<ChatCompletionPresetsDbModel> AddChatCompletionPresetAsync(ChatCompletionPresetsDbModel dbModel);
        Task<bool> DeleteChatCompletionPresetAsync(string chatCompletionPresetId);
        Task<ChatCompletionPresetsDbModel[]> GetChatCompletionPresetsAsync();
        Task<ChatCompletionPresetsDbModel> GetChatCompletionPresetAsync(string chatCompletionPresetId);
        Task<bool> UpdateChatCompletionPresetAsync(ChatCompletionPresetsDbModel currentChatCompletionPreset);
    }
}
