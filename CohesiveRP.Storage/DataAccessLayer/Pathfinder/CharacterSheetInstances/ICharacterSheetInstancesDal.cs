using CohesiveRP.Storage.DataAccessLayer.Chats;

namespace CohesiveRP.Storage.DataAccessLayer.Users
{
    public interface ICharacterSheetInstancesDal
    {
        Task<CharacterSheetInstancesDbModel[]> GetCharacterSheetInstancesAsync();
        Task<CharacterSheetInstancesDbModel> GetCharacterSheetsInstanceByChatIdAsync(string chatId);
        Task<CharacterSheetInstancesDbModel[]> GetCharacterSheetInstancesByFuncAsync(Func<CharacterSheetInstancesDbModel, bool> func);
        Task<CharacterSheetInstancesDbModel> AddCharacterSheetsInstanceAsync(CharacterSheetInstancesDbModel dbModel);
        Task<bool> UpdateCharacterSheetsInstanceAsync(CharacterSheetInstancesDbModel dbModel);
        Task<bool> DeleteCharacterSheetsInstanceAsync(CharacterSheetInstancesDbModel dbModel);
    }
}
