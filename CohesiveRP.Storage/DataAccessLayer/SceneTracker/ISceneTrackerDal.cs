using CohesiveRP.Storage.DataAccessLayer.Messages;
using CohesiveRP.Storage.QueryModels.SceneTracker;

namespace CohesiveRP.Storage.DataAccessLayer.SceneTracker
{
    public interface ISceneTrackerDal
    {
        Task<SceneTrackerDbModel> AddSceneTracker(CreateSceneTrackerQueryModel queryModel);
        Task<SceneTrackerDbModel> CreateOrUpdateSceneTracker(CreateSceneTrackerQueryModel queryModel);
        Task<SceneTrackerDbModel> GetSceneTracker(string chatId);
    }
}
