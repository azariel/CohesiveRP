using CohesiveRP.Storage.DataAccessLayer.Messages;
using CohesiveRP.Storage.QueryModels.SceneTracker;

namespace CohesiveRP.Storage.DataAccessLayer.SceneTracker
{
    public interface ISceneTrackerDal
    {
        Task<SceneTrackerDbModel> AddSceneTrackerAsync(CreateSceneTrackerQueryModel queryModel);
        Task<SceneTrackerDbModel> CreateOrUpdateSceneTrackerAsync(CreateSceneTrackerQueryModel queryModel);
        Task<bool> DeleteSceneTrackerAsync(string chatId);
        Task<SceneTrackerDbModel> GetSceneTrackerAsync(string chatId);
    }
}
