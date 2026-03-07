using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Settings;

namespace CohesiveRP.Core.Services.Summary
{
    public interface ISummaryService
    {
        Task EvaluateSummaryAsync(string chatId, PromptContextSettings settings);
    }
}
