namespace CohesiveRP.Storage.DataAccessLayer.ChatAdditions.NarrativeArchitecture
{
    public interface INarrativeArchitecturesDal
    {
        Task<NarrativeArchitectureDbModel> AddNarrativeArchitectureAsync(NarrativeArchitectureDbModel queryModel);
        Task<NarrativeArchitectureDbModel> UpdateNarrativeArchitectureAsync(NarrativeArchitectureDbModel queryModel);
        Task<bool> DeleteNarrativeArchitectureAsync(Func<NarrativeArchitectureDbModel, bool> func);
        Task<NarrativeArchitectureDbModel[]> GetNarrativeArchitecturesAsync(Func<NarrativeArchitectureDbModel, bool> func);
    }
}
