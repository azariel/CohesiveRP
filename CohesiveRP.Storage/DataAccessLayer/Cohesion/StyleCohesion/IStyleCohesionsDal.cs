using CohesiveRP.Storage.DataAccessLayer.Cohesion.CharactersCohesionEnforcement;

namespace CohesiveRP.Storage.DataAccessLayer.Cohesion.StyleCohesion
{
    public interface IStyleCohesionsDal
    {
        Task<StyleCohesionDbModel> AddStyleCohesionAsync(StyleCohesionDbModel queryModel);
        Task<StyleCohesionDbModel> UpdateStyleCohesionAsync(StyleCohesionDbModel queryModel);
        Task<bool> DeleteStyleCohesionAsync(Func<StyleCohesionDbModel, bool> func);
        Task<StyleCohesionDbModel[]> GetStyleCohesionsAsync(Func<StyleCohesionDbModel, bool> func);
    }
}
