using CohesiveRP.Storage.DataAccessLayer.Chats;

namespace CohesiveRP.Storage.DataAccessLayer.Users
{
    public interface ICharacterSheetsDal
    {
        Task<CharacterSheetDbModel[]> GetCharacterSheetsAsync();
        Task<CharacterSheetDbModel> GetCharacterSheetByCharacterIdAsync(string characterSheetId);
        Task<CharacterSheetDbModel[]> GetCharacterSheetsByFuncAsync(Func<CharacterSheetDbModel, bool> func);
        Task<CharacterSheetDbModel> AddCharacterSheetAsync(CharacterSheetDbModel dbModel);
        Task<bool> UpdateCharacterSheetAsync(CharacterSheetDbModel dbModel);
        Task<bool> DeleteCharacterSheetAsync(CharacterSheetDbModel dbModel);
    }
}
