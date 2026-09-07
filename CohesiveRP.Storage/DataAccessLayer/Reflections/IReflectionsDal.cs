namespace CohesiveRP.Storage.DataAccessLayer.ChatAdditions.Reflection
{
    public interface IReflectionsDal
    {
        // Reflection
        Task<ReflectionDbModel> AddReflectionAsync(ReflectionDbModel queryModel);
        Task<ReflectionDbModel> UpdateReflectionAsync(ReflectionDbModel queryModel);
        Task<bool> DeleteReflectionAsync(Func<ReflectionDbModel, bool> func);
        Task<ReflectionDbModel[]> GetReflectionsAsync(Func<ReflectionDbModel, bool> func);
    }
}
