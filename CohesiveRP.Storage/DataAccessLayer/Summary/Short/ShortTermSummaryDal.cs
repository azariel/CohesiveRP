using System.Text.Json;
using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.Serialization;
using CohesiveRP.Storage.Common;
using CohesiveRP.Storage.DataAccessLayer.Messages;
using CohesiveRP.Storage.DataAccessLayer.Users;
using CohesiveRP.Storage.QueryModels.Message;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace CohesiveRP.Storage.DataAccessLayer.Summary.Short
{
    /// <summary>
    /// DataAccessLayer around Chats.
    /// </summary>
    public class ShortTermSummaryDal : StorageDal, IShortTermSummaryDal
    {
        private readonly IDbContextFactory<StorageDbContext> contextFactory;

        public ShortTermSummaryDal(JsonSerializerOptions jsonSerializerOptions, IDbContextFactory<StorageDbContext> contextFactory) : base(jsonSerializerOptions)
        {
            this.contextFactory = contextFactory;

            using var dbContext = contextFactory.CreateDbContext();
            dbContext.Database.EnsureCreated();
        }

        public async Task<ISummaryDbModel> AddShortTermSummaryAsync(CreateSummaryQueryModel queryModel)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();

                // Convert model
                SummaryDbModel summaryDbModel = new SummaryDbModel
                {
                    MessageIdTracker = queryModel.MessageIdTracker,
                    Content = queryModel.Content,
                    CreatedAtUtc = queryModel.CreatedAtUtc,
                };

                // Check if a short-term summary for this chat already exist
                ShortTermSummaryDbModel shortTermSummaryDbModel = dbContext.ShortTermSummaries.FirstOrDefault(f => f.ChatId == queryModel.ChatId);

                if (shortTermSummaryDbModel == null)
                {
                    // Create the Short-term summary row tied to this chat first
                    var newShortTermSummaryObj = new ShortTermSummaryDbModel
                    {
                        SummaryId = Guid.NewGuid().ToString(),
                        ChatId = queryModel.ChatId,
                        InsertDateTimeUtc = DateTime.UtcNow,
                        Summaries = new List<SummaryDbModel> { summaryDbModel },
                    };

                    EntityEntry<ShortTermSummaryDbModel> resultAdd = dbContext.ShortTermSummaries.Add(newShortTermSummaryObj);
                    if (resultAdd.State != EntityState.Added)
                    {
                        LoggingManager.LogToFile("3f149f2b-5f73-4ca6-bf33-905d0a02aba4", $"Error when querying Db on table ShortTermSummaries. State was [{resultAdd.State}]. Result: [{JsonCommonSerializer.SerializeToString(resultAdd)}].");
                        return null;
                    }

                    await dbContext.SaveChangesAsync();
                    return summaryDbModel;
                }

                // Otherwise, Update the existing row with the new message
                List<SummaryDbModel> currentSummaries = shortTermSummaryDbModel.Summaries;
                currentSummaries.Add(summaryDbModel);

                shortTermSummaryDbModel.Summaries = currentSummaries;
                EntityEntry<ShortTermSummaryDbModel> resultUpdate = dbContext.ShortTermSummaries.Update(shortTermSummaryDbModel);
                if (resultUpdate.State != EntityState.Modified)
                {
                    LoggingManager.LogToFile("da38c92d-10f3-4809-a7f2-d7312453521f", $"Error when querying Db on table ShortTermSummaries. State was [{resultUpdate.State}]. Result: [{JsonCommonSerializer.SerializeToString(resultUpdate)}].");
                    return null;
                }

                await dbContext.SaveChangesAsync();
                return summaryDbModel;
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("a5b8aa90-68de-45a0-bf12-49e3c95b5f94", $"Error when querying Db on table ShortTermSummaries.", ex);
                return null;
            }
        }

        public async Task<ShortTermSummaryDbModel> GetShortTermSummaryAsync(string chatId)
        {
            try
            {
                using var dbContext = await contextFactory.CreateDbContextAsync();
                return dbContext.ShortTermSummaries.FirstOrDefault(w => w.ChatId == chatId);
            } catch (Exception ex)
            {
                LoggingManager.LogToFile("4136e601-ec63-42b4-bee7-ec39b742bbf7", $"Error when querying Db on table ShortTermSummaries.", ex);
                return null;
            }
        }
    }
}
