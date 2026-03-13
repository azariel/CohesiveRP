using CohesiveRP.Storage.DataAccessLayer.Chats;
using CohesiveRP.Storage.QueryModels.Personas;

namespace CohesiveRP.Storage.DataAccessLayer.Users
{
    public interface IPersonasDal
    {
        Task<PersonaDbModel[]> GetPersonasAsync();
        Task<PersonaDbModel> GetPersonaByIdAsync(string personaId);
        Task<PersonaDbModel> AddPersonaAsync(AddPersonaQueryModel queryModel);
        Task<bool> UpdatePersonaAsync(PersonaDbModel personaDbModel);
        Task<bool> DeletePersonaAsync(PersonaDbModel personaDbModel);
    }
}
