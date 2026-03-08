using CohesiveRP.Storage.DataAccessLayer.Settings;

namespace CohesiveRP.Core.Services.Summary
{
    public interface ISummaryService
    {
        Task EvaluateSummaryAsync(string chatId, GlobalSettingsDbModel settings);
    }
}
