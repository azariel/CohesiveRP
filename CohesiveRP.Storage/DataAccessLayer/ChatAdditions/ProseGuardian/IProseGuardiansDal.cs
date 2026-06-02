namespace CohesiveRP.Storage.DataAccessLayer.ChatAdditions.ProseGuardian
{
    public interface IProseGuardiansDal
    {
        // ProseGuardian
        Task<ProseGuardianDbModel> AddProseGuardianAsync(ProseGuardianDbModel queryModel);
        Task<ProseGuardianDbModel> UpdateProseGuardianAsync(ProseGuardianDbModel queryModel);
        Task<bool> DeleteProseGuardianAsync(Func<ProseGuardianDbModel, bool> func);
        Task<ProseGuardianDbModel[]> GetProseGuardiansAsync(Func<ProseGuardianDbModel, bool> func);
    }
}
