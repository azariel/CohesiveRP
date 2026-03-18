using CohesiveRP.Core.CharacterCards.Loaders.CCv3.BusinessObjects;
using CohesiveRP.Core.DtoConverters.Abstractions;
using CohesiveRP.Core.Lorebooks;
using CohesiveRP.Core.Lorebooks.Loaders.CCv3.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.Chats;
using CohesiveRP.Storage.DataAccessLayer.Lorebooks.BusinessObjects;

namespace CohesiveRP.Core.DtoConverters
{
    public class LorebookDtoConverter : ILorebookDtoConverter
    {
        public LorebookDbModel Convert(ILorebook lorebook)
        {
            if (lorebook == null)
            {
                return null;
            }

            switch (lorebook)
            {
                case CCv3CharacterLorebook characterLorebook:
                    return ConvertLorebookFromCharacter(characterLorebook);
                case CCv3Lorebook lorebookLorebook:
                    return ConvertLorebookFromLorebook(lorebookLorebook);
            }

            return null;
        }

        public LorebookDbModel ConvertLorebookFromCharacter(CCv3CharacterLorebook lorebook)
        {
            return new LorebookDbModel()
            {
                Name = lorebook.Name,
                Entries = lorebook.Entries.Select(s => new LorebookEntry
                {
                    Enabled = s.Enabled,
                    Constant = s.Constant,
                    Content = s.Content,
                    InsertionOrder = s.InsertionOrder,
                    Keys = s.Keys,
                    UseRegex = s.UseRegex,

                    // Fields unsupported by the input format
                    Name = s.Keys.FirstOrDefault() ?? "",
                    Depth = 4,// Nb messages to parse to determine if the entry triggers or not
                    CaseSensitive = false,
                }).ToList(),
            };
        }

        public LorebookDbModel ConvertLorebookFromLorebook(CCv3Lorebook lorebook)
        {
            return new LorebookDbModel()
            {
                Name = lorebook.Name,
                Entries = lorebook.Entries.Select(s => new LorebookEntry
                {
                    Enabled = s.Value.Enabled,
                    Constant = s.Value.Constant,
                    Content = s.Value.Content,
                    Name = s.Value.Name,
                    InsertionOrder = s.Value.InsertionOrder,
                    Keys = s.Value.Keys,
                    Depth = s.Value.Depth,
                    CaseSensitive = s.Value.CaseSensitive ?? false,

                    // Fields unsupported by the input format
                    UseRegex = false,
                }).ToList(),
            };
        }
    }
}
