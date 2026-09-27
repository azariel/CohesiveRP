using CohesiveRP.Storage.DataAccessLayer.Messages;

namespace CohesiveRP.Storage.DataAccessLayer.SceneTracker
{
    public interface ISceneTrackerDal
    {
        Task<SceneTrackerDbModel> AddSceneTrackerAsync(SceneTrackerDbModel queryModel);
        Task<SceneTrackerDbModel> CreateOrUpdateSceneTrackerAsync(SceneTrackerDbModel queryModel, bool autoUpdatePreviousContent = true);
        Task<bool> DeleteSceneTrackerAsync(string chatId);
        Task<SceneTrackerDbModel> GetSceneTrackerAsync(string chatId);
    }
}
