namespace CohesiveRP.Storage.DataAccessLayer.ChatAdditions.CohesionEnforcement
{
    public interface ICohesionEnforcementsDal
    {
        // CohesionEnforcer
        Task<CohesionEnforcementDbModel> AddCohesionEnforcementAsync(CohesionEnforcementDbModel queryModel);
        Task<CohesionEnforcementDbModel> UpdateCohesionEnforcementAsync(CohesionEnforcementDbModel queryModel);
        Task<bool> DeleteCohesionEnforcementAsync(Func<CohesionEnforcementDbModel, bool> func);
        Task<CohesionEnforcementDbModel[]> GetCohesionEnforcementsAsync(Func<CohesionEnforcementDbModel, bool> func);
    }
}
