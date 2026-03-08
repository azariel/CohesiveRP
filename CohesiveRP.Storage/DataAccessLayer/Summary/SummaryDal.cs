using System.Text.Json;
using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.Serialization;
using CohesiveRP.Storage.Common;
using CohesiveRP.Storage.DataAccessLayer.BackgroundQueries.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.Messages;
using CohesiveRP.Storage.DataAccessLayer.Users;
using CohesiveRP.Storage.QueryModels.Message;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace CohesiveRP.Storage.DataAccessLayer.Summary.Short
{
    /// <summary>
    /// DataAccessLayer around Summaries.
    /// </summary>
    public class SummaryDal : StorageDal, ISummaryDal
    {
        private readonly IDbContextFactory<StorageDbContext> contextFactory;

        private enum SpecificTermSummary
        {
            Short,
            Medium,
            Long,
            Extra,
            Overflow
        }

        public SummaryDal(JsonSerializerOptions jsonSerializerOptions, IDbContextFactory<StorageDbContext> contextFactory) : base(jsonSerializerOptions)
        {
            this.contextFactory = contextFactory;

            using var dbContext = contextFactory.CreateDbContext();
            dbContext.Database.EnsureCreated();
        }

        private async Task<ISummaryEntryDbModel> AddSpecificTermSummaryAsync(CreateSummaryQueryModel queryModel, SpecificTermSummary termSummary)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();

                // Convert model
                SummaryEntryDbModel summaryEntryDbModel = new SummaryEntryDbModel
                {
                    SummaryEntryId = Guid.NewGuid().ToString(),
                    MessageIdTracker = queryModel.MessageIdTracker,
                    Content = queryModel.Content,
                    CreatedAtUtc = queryModel.CreatedAtUtc,
                };

                // Check if a short-term summary for this chat already exist
                SummaryDbModel summaryDbModel = dbContext.Summaries.FirstOrDefault(f => f.ChatId == queryModel.ChatId);

                if (summaryDbModel == null)
                {
                    // Create the Short-term summary row tied to this chat first
                    var summaryObj = new SummaryDbModel
                    {
                        SummaryId = Guid.NewGuid().ToString(),
                        ChatId = queryModel.ChatId,
                        InsertDateTimeUtc = DateTime.UtcNow,
                    };

                    switch (termSummary)
                    {
                        case SpecificTermSummary.Short:
                            summaryObj.ShortTermSummaries = [summaryEntryDbModel];
                            break;
                        case SpecificTermSummary.Medium:
                            summaryObj.MediumTermSummaries = [summaryEntryDbModel];
                            break;
                        case SpecificTermSummary.Long:
                            summaryObj.LongTermSummaries = [summaryEntryDbModel];
                            break;
                        case SpecificTermSummary.Extra:
                            summaryObj.ExtraTermSummaries = [summaryEntryDbModel];
                            break;
                        case SpecificTermSummary.Overflow:
                            summaryObj.OverflowTermSummaries = [summaryEntryDbModel];
                            break;
                        default:
                            break;
                    }

                    EntityEntry<SummaryDbModel> resultAdd = dbContext.Summaries.Add(summaryObj);
                    if (resultAdd.State != EntityState.Added)
                    {
                        LoggingManager.LogToFile("3f149f2b-5f73-4ca6-bf33-905d0a02aba4", $"Error when querying Db on table Summaries. State was [{resultAdd.State}]. Result: [{JsonCommonSerializer.SerializeToString(resultAdd)}].");
                        return null;
                    }

                    await dbContext.SaveChangesAsync();
                    return summaryEntryDbModel;
                }

                // Otherwise, Update the existing row with the new message
                switch (termSummary)
                {
                    case SpecificTermSummary.Short:
                        List<SummaryEntryDbModel> currentShortSummaries = summaryDbModel.ShortTermSummaries ?? new();
                        currentShortSummaries.Add(summaryEntryDbModel);
                        summaryDbModel.ShortTermSummaries = currentShortSummaries;
                        break;
                    case SpecificTermSummary.Medium:
                        List<SummaryEntryDbModel> currentMediumSummaries = summaryDbModel.MediumTermSummaries ?? new();
                        currentMediumSummaries.Add(summaryEntryDbModel);
                        summaryDbModel.MediumTermSummaries = currentMediumSummaries;
                        break;
                    case SpecificTermSummary.Long:
                        List<SummaryEntryDbModel> currentLongSummaries = summaryDbModel.LongTermSummaries ?? new();
                        currentLongSummaries.Add(summaryEntryDbModel);
                        summaryDbModel.LongTermSummaries = currentLongSummaries;
                        break;
                    case SpecificTermSummary.Extra:
                        List<SummaryEntryDbModel> currentExtraSummaries = summaryDbModel.ExtraTermSummaries ?? new();
                        currentExtraSummaries.Add(summaryEntryDbModel);
                        summaryDbModel.ExtraTermSummaries = currentExtraSummaries;
                        break;
                    case SpecificTermSummary.Overflow:
                        List<SummaryEntryDbModel> currentOverflowSummaries = summaryDbModel.OverflowTermSummaries ?? new();
                        currentOverflowSummaries.Add(summaryEntryDbModel);
                        summaryDbModel.OverflowTermSummaries = currentOverflowSummaries;
                        break;
                    default:
                        break;
                }

                EntityEntry<SummaryDbModel> resultUpdate = dbContext.Summaries.Update(summaryDbModel);
                if (resultUpdate.State != EntityState.Modified)
                {
                    LoggingManager.LogToFile("da38c92d-10f3-4809-a7f2-d7312453521f", $"Error when querying Db on table Summaries. State was [{resultUpdate.State}]. Result: [{JsonCommonSerializer.SerializeToString(resultUpdate)}].");
                    return null;
                }

                await dbContext.SaveChangesAsync();
                return summaryEntryDbModel;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("a5b8aa90-68de-45a0-bf12-49e3c95b5f94", $"Error when adding new summary on table Summaries.", ex);
                return null;
            }
        }

        public async Task<ISummaryEntryDbModel> AddShortTermSummaryAsync(CreateSummaryQueryModel queryModel) => await AddSpecificTermSummaryAsync(queryModel, SpecificTermSummary.Short);
        public async Task<ISummaryEntryDbModel> AddMediumTermSummaryAsync(CreateSummaryQueryModel queryModel) => await AddSpecificTermSummaryAsync(queryModel, SpecificTermSummary.Medium);
        public async Task<ISummaryEntryDbModel> AddLongTermSummaryAsync(CreateSummaryQueryModel queryModel) => await AddSpecificTermSummaryAsync(queryModel, SpecificTermSummary.Long);
        public async Task<ISummaryEntryDbModel> AddExtraTermSummaryAsync(CreateSummaryQueryModel queryModel) => await AddSpecificTermSummaryAsync(queryModel, SpecificTermSummary.Extra);
        public async Task<ISummaryEntryDbModel> AddOverflowTermSummaryAsync(CreateSummaryQueryModel queryModel) => await AddSpecificTermSummaryAsync(queryModel, SpecificTermSummary.Overflow);

        public async Task<SummaryDbModel> GetSummaryAsync(string chatId)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                return dbContext.Summaries.FirstOrDefault(w => w.ChatId == chatId);
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("4136e601-ec63-42b4-bee7-ec39b742bbf7", $"Error when querying Db on table Summaries.", ex);
                return null;
            }
        }

        private async Task<bool> DeleteSummaryAsync(string chatId, string[] summariesIds, BackgroundQuerySystemTags tag)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();

                SummaryDbModel dbModel = dbContext.Summaries.FirstOrDefault(f => f.ChatId == chatId);

                if (dbModel == null)
                {
                    LoggingManager.LogToFile("ca8936af-8434-46d8-9f1f-5a6d8e0499bb", $"Error when deleting a query on table summaries. Couldn't find summary row tied to chatId [{chatId}]. Aborting.");
                    return false;
                }

                switch (tag)
                {
                    case BackgroundQuerySystemTags.shortSummary:
                        dbModel.ShortTermSummaries.RemoveAll(w => summariesIds.Any(a => a == w.SummaryEntryId));
                        break;
                    case BackgroundQuerySystemTags.mediumSummary:
                        dbModel.MediumTermSummaries.RemoveAll(w => summariesIds.Any(a => a == w.SummaryEntryId));
                        break;
                    case BackgroundQuerySystemTags.longSummary:
                        dbModel.LongTermSummaries.RemoveAll(w => summariesIds.Any(a => a == w.SummaryEntryId));
                        break;
                    case BackgroundQuerySystemTags.extraSummary:
                        dbModel.ExtraTermSummaries.RemoveAll(w => summariesIds.Any(a => a == w.SummaryEntryId));
                        break;
                    case BackgroundQuerySystemTags.overflowSummary:
                        dbModel.OverflowTermSummaries.RemoveAll(w => summariesIds.Any(a => a == w.SummaryEntryId));
                        break;
                    default:
                        LoggingManager.LogToFile("f8867d81-577c-4dba-b23a-adf4bb14e0e1", $"Error when deleting a query on table summaries. Unhandled tag [{tag}]. Aborting.");
                        return false;

                }

                EntityEntry<SummaryDbModel> resultUpdate = dbContext.Summaries.Update(dbModel);
                if (resultUpdate.State != EntityState.Modified)
                {
                    LoggingManager.LogToFile("683fb56b-0d30-41ce-8407-82ef977493dc", $"Error when querying Db on table Summaries. State was [{resultUpdate.State}]. Result: [{JsonCommonSerializer.SerializeToString(resultUpdate)}].");
                    return false;
                }

                await dbContext.SaveChangesAsync();
                return true;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("4e6e4272-f8a6-487b-8367-a43c4f67b10e", $"Error when deleting specific summaries on table Summaries.", ex);
                return false;
            }
        }

        public async Task<bool> DeleteShortTermSummariesEntriesAsync(string chatId, string[] summariesIds) => await DeleteSummaryAsync(chatId, summariesIds, BackgroundQuerySystemTags.shortSummary);
        public async Task<bool> DeleteMediumTermSummariesEntriesAsync(string chatId, string[] summariesIds) => await DeleteSummaryAsync(chatId, summariesIds, BackgroundQuerySystemTags.mediumSummary);
        public async Task<bool> DeleteLongTermSummariesEntriesAsync(string chatId, string[] summariesIds) => await DeleteSummaryAsync(chatId, summariesIds, BackgroundQuerySystemTags.longSummary);
        public async Task<bool> DeleteExtraTermSummariesEntriesAsync(string chatId, string[] summariesIds) => await DeleteSummaryAsync(chatId, summariesIds, BackgroundQuerySystemTags.extraSummary);
        public async Task<bool> DeleteOverflowTermSummariesEntriesAsync(string chatId, string[] summariesIds) => await DeleteSummaryAsync(chatId, summariesIds, BackgroundQuerySystemTags.overflowSummary);
    }
}
