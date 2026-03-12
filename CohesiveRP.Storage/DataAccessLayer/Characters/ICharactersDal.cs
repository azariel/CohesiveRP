using CohesiveRP.Storage.DataAccessLayer.Chats;
using CohesiveRP.Storage.QueryModels.Chat;

namespace CohesiveRP.Storage.DataAccessLayer.Users
{
    public interface ICharactersDal
    {
        Task<CharacterDbModel[]> GetCharactersAsync();
        Task<CharacterDbModel> GetCharacterByIdAsync(string characterId);
        Task<CharacterDbModel> AddCharacterAsync(AddCharacterQueryModel queryModel);
        Task<bool> UpdateCharacter(CharacterDbModel characterDbModel);
    }
}
