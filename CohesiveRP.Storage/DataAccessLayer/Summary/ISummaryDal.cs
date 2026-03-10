using CohesiveRP.Storage.DataAccessLayer.Messages;
using CohesiveRP.Storage.QueryModels.Message;

namespace CohesiveRP.Storage.DataAccessLayer.Summary.Short
{
    public interface ISummaryDal
    {
        Task<ISummaryEntryDbModel> AddShortTermSummaryAsync(CreateSummaryQueryModel queryModel);
        Task<ISummaryEntryDbModel> AddMediumTermSummaryAsync(CreateSummaryQueryModel queryModel);
        Task<ISummaryEntryDbModel> AddLongTermSummaryAsync(CreateSummaryQueryModel queryModel);
        Task<ISummaryEntryDbModel> AddExtraTermSummaryAsync(CreateSummaryQueryModel queryModel);
        Task<ISummaryEntryDbModel> AddOverflowTermSummaryAsync(CreateSummaryQueryModel queryModel);
        Task<SummaryDbModel> GetSummaryAsync(string chatId);
        Task<bool> DeleteShortTermSummariesEntriesAsync(string chatId, string[] summariesIds);
        Task<bool> DeleteMediumTermSummariesEntriesAsync(string chatId, string[] summariesIds);
        Task<bool> DeleteLongTermSummariesEntriesAsync(string chatId, string[] summariesIds);
        Task<bool> DeleteExtraTermSummariesEntriesAsync(string chatId, string[] summariesIds);
        Task<bool> DeleteOverflowTermSummariesEntriesAsync(string chatId, string[] summariesIds);
    }
}