using CohesiveRP.Common;
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
                    Comment = "",
                    SecondaryKeys = [],
                    Vectorized = false,
                    EntryId = Guid.NewGuid().ToString(),
                    SelectiveLogicBetweenKeysAndSecondaryKeys = KeysEvaluationLogicGate.MainKeysOnly,
                    MatchWholeWord = false,
                    ProbabilityPercentage = 100,
                    PositionInPrompt = 4,
                    StickyForNbMessages = 3,
                    Cooldown = 0,
                    Delay = 0,
                    IgnoreTokensBudget = false,
                    ExcludeRecursion = true,
                    PreventRecursion = false,
                    OnlyTriggeredByRecursion = false,
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
                    Enabled = !s.Value.Disable,
                    Constant = s.Value.Constant,
                    Content = s.Value.Content,
                    Name = s.Value.Name,
                    InsertionOrder = s.Value.InsertionOrder,
                    Depth = s.Value.Depth,
                    CaseSensitive = (s.Value.CaseSensitive ?? false) || (s.Value.CaseSensitiveLegacy ?? false),
                    Comment = s.Value.Comment,
                    Keys = [..s.Value.Keys, ..s.Value.Key],// merge those fields together
                    SecondaryKeys = [..s.Value.SecondaryKeys, ..s.Value.KeySecondary],// merge those two fields into a single one
                    Vectorized = s.Value.Vectorized,
                    SelectiveLogicBetweenKeysAndSecondaryKeys = !s.Value.Selective ? KeysEvaluationLogicGate.MainKeysOnly : (s.Value.SelectiveLogic == 0 ? KeysEvaluationLogicGate.AND : KeysEvaluationLogicGate.OR),
                    MatchWholeWord = s.Value.MatchWholeWords ?? false,
                    ProbabilityPercentage = s.Value.Probability,
                    PositionInPrompt = s.Value.Position,
                    StickyForNbMessages = s.Value.Sticky,
                    Cooldown = s.Value.Cooldown,
                    Delay = s.Value.Delay,
                    IgnoreTokensBudget = s.Value.IgnoreBudget,
                    ExcludeRecursion = s.Value.ExcludeRecursion,
                    PreventRecursion = s.Value.PreventRecursion,
                    OnlyTriggeredByRecursion = s.Value.DelayUntilRecursion,

                    // Fields unsupported by the input format
                    UseRegex = false,
                    EntryId = Guid.NewGuid().ToString(),
                }).ToList(),
            };
        }
    }
}
