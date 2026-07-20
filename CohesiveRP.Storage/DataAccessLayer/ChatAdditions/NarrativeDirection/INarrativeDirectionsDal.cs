namespace CohesiveRP.Storage.DataAccessLayer.ChatAdditions.NarrativeDirection
{
    public interface INarrativeDirectionsDal
    {
        // NarrativeDirector
        Task<NarrativeDirectionDbModel> AddNarrativeDirectionAsync(NarrativeDirectionDbModel queryModel);
        Task<NarrativeDirectionDbModel> UpdateNarrativeDirectionAsync(NarrativeDirectionDbModel queryModel);
        Task<bool> DeleteNarrativeDirectionAsync(Func<NarrativeDirectionDbModel, bool> func);
        Task<NarrativeDirectionDbModel[]> GetNarrativeDirectionsAsync(Func<NarrativeDirectionDbModel, bool> func);
    }
}
