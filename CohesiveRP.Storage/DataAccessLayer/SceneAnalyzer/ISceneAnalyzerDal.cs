using CohesiveRP.Storage.DataAccessLayer.Messages;

namespace CohesiveRP.Storage.DataAccessLayer.SceneAnalyzer
{
    public interface ISceneAnalyzerDal
    {
        Task<SceneAnalyzerDbModel> AddSceneAnalyzerAsync(SceneAnalyzerDbModel dbModel);
        Task<SceneAnalyzerDbModel> CreateOrUpdateSceneAnalyzerAsync(SceneAnalyzerDbModel dbModel);
        Task<bool> DeleteSceneAnalyzerAsync(string chatId);
        Task<SceneAnalyzerDbModel> GetSceneAnalyzerAsync(string chatId);
    }
}
