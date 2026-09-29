namespace CohesiveRP.Storage.DataAccessLayer.Cohesion.CharactersCohesionEnforcement
{
    public interface ICharactersCohesionEnforcementsDal
    {
        Task<CharactersCohesionEnforcementDbModel> AddCharactersCohesionEnforcementAsync(CharactersCohesionEnforcementDbModel queryModel);
        Task<CharactersCohesionEnforcementDbModel> UpdateCharactersCohesionEnforcementAsync(CharactersCohesionEnforcementDbModel queryModel);
        Task<bool> DeleteCharactersCohesionEnforcementAsync(Func<CharactersCohesionEnforcementDbModel, bool> func);
        Task<CharactersCohesionEnforcementDbModel[]> GetCharactersCohesionEnforcementsAsync(Func<CharactersCohesionEnforcementDbModel, bool> func);
    }
}
