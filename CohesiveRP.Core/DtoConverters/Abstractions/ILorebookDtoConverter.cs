using CohesiveRP.Core.Lorebooks;
using CohesiveRP.Storage.DataAccessLayer.Chats;

namespace CohesiveRP.Core.DtoConverters.Abstractions
{
    public interface ILorebookDtoConverter
    {
        LorebookDbModel Convert(ILorebook lorebook);
    }
}
