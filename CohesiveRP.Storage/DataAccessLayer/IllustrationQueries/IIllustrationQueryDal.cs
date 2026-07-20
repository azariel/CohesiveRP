namespace CohesiveRP.Storage.DataAccessLayer.InteractiveUserInputQueries
{
    public interface IIllustrationQueryDal
    {
        Task<IllustrationQueryDbModel[]> GetIllustrationQueriesAsync(Func<IllustrationQueryDbModel, bool> func);
        Task<IllustrationQueryDbModel> AddIllustrationQueryAsync(IllustrationQueryDbModel illustrationQueryDbModel);
        Task<bool> UpdateIllustrationQueryAsync(IllustrationQueryDbModel illustrationQueryDbModel);
        Task<bool> DeleteIllustrationQueryAsync(string illustrationQueryId);
        Task<bool> DeleteIllustrationQueryAsync(Func<IllustrationQueryDbModel, bool> func);
    }
}
