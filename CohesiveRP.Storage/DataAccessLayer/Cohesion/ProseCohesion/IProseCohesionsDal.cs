using CohesiveRP.Storage.DataAccessLayer.Cohesion.CharactersCohesionEnforcement;

namespace CohesiveRP.Storage.DataAccessLayer.Cohesion.ProseCohesion
{
    public interface IProseCohesionDal
    {
        Task<ProseCohesionDbModel> AddProseCohesionAsync(ProseCohesionDbModel queryModel);
        Task<ProseCohesionDbModel> UpdateProseCohesionAsync(ProseCohesionDbModel queryModel);
        Task<bool> DeleteProseCohesionAsync(Func<ProseCohesionDbModel, bool> func);
        Task<ProseCohesionDbModel[]> GetProseCohesionsAsync(Func<ProseCohesionDbModel, bool> func);
    }
}
