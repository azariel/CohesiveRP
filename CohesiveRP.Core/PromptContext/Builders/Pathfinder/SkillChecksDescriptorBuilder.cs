using System.Text;
using CohesiveRP.Core.PromptContext.Abstractions;
using CohesiveRP.Core.PromptContext.Utils;
using CohesiveRP.Core.Services;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Format;
using CohesiveRP.Storage.DataAccessLayer.Chats;
using CohesiveRP.Storage.DataAccessLayer.Messages;

namespace CohesiveRP.Core.PromptContext.Builders.Pathfinder
{
    public class SkillChecksDescriptorBuilder : IPromptContextElementBuilder
    {
        private IStorageService storageService;
        private PromptContextFormatElement promptContextFormatElement;
        private ChatDbModel chatDbModel;
        private PersonaDbModel personaLinkedToChat;
        private CharacterDbModel[] charactersLinkedToChat;

        public SkillChecksDescriptorBuilder(IStorageService storageService, PromptContextFormatElement promptContextFormatElement, ChatDbModel chatDbModel, PersonaDbModel personaLinkedToChat, CharacterDbModel[] charactersLinkedToChat)
        {
            this.storageService = storageService;
            this.promptContextFormatElement = promptContextFormatElement;
            this.chatDbModel = chatDbModel;
            this.personaLinkedToChat = personaLinkedToChat;
            this.charactersLinkedToChat = charactersLinkedToChat;
        }

        public async Task<(string, IShareableContextLink)> BuildAsync()
        {
            var hotMessagesObj = await storageService.GetAllHotMessagesAsync(chatDbModel.ChatId);

            if (hotMessagesObj?.Messages == null || hotMessagesObj.Messages.Count < 2)
            {
                return (null, new ShareableContextLink { LinkedBuilder = this, });
            }

            var rolls = await storageService.GetChatCharactersRollsByChatIdAsync(chatDbModel.ChatId);
            if (rolls?.ChatCharactersRolls == null || rolls.ChatCharactersRolls.Count <= 0)
            {
                return (null, new ShareableContextLink { LinkedBuilder = this, });
            }

            MessageDbModel[] hotMessages = hotMessagesObj.Messages.OrderByDescending(o => o.CreatedAtUtc).ToArray();
            var hotMessagesBesideLastOne = hotMessages.Skip(1).ToArray();
            int nbMessagesGeneralContext = hotMessages.IndexOf(hotMessagesBesideLastOne.FirstOrDefault(f => f.SourceType == Common.BusinessObjects.MessageSourceType.User));
            int nbMessagesRequest = Math.Max(1, nbMessagesGeneralContext);
            nbMessagesGeneralContext = Math.Max(5, nbMessagesGeneralContext);

            MessageDbModel[] LastXMessagesforGeneralContext = [.. hotMessagesBesideLastOne.Take(nbMessagesGeneralContext)];
            string contextOnScene = string.Join($"{Environment.NewLine}", LastXMessagesforGeneralContext.OrderBy(o => o.CreatedAtUtc).Select(s => $"<message>{s.Content}</message>"));

            MessageDbModel[] LastXMessagesforRequest = [.. hotMessages.Take(nbMessagesRequest)];
            string lastMessage = string.Join($"{Environment.NewLine}", LastXMessagesforRequest.OrderBy(o => o.CreatedAtUtc).Select(s => $"<message>{s.Content.InjectMacros(personaLinkedToChat?.Name, charactersLinkedToChat?.FirstOrDefault()?.Name)}</message>"));

            // rolls
            var chatCharacterSheetInstance = await storageService.GetCharacterSheetsInstanceByChatIdAsync(chatDbModel.ChatId);
            var persona = chatCharacterSheetInstance?.CharacterSheetInstances?.FirstOrDefault(w => w.PersonaId != null);
            if (persona == null)
            {
                return (null, new ShareableContextLink { LinkedBuilder = this, });
            }

            // add the roll where the player initiates
            var rollsWithPlayer = rolls.ChatCharactersRolls.Where(w => w.CharacterSheetInstanceId == persona.CharacterSheetInstanceId).ToList();

            // add the rolls where the player is a target
            foreach (var roll in rolls.ChatCharactersRolls.Where(w => w.CharacterSheetInstanceId != persona.CharacterSheetInstanceId))
            {
                if (roll.Rolls == null || roll.Rolls.Count <= 0)
                {
                    continue;
                }

                if (roll.Rolls.Any(w => w.CharactersInScene.Any(a => a.CharacterSheetInstanceId == persona.CharacterSheetInstanceId)))
                {
                    rollsWithPlayer.Add(roll);
                }
            }

            if (rollsWithPlayer == null || rollsWithPlayer.Count <= 0)
            {
                return (null, new ShareableContextLink { LinkedBuilder = this, });
            }

            StringBuilder str = new();
            str.AppendLine("<rolls>");
            foreach (var rollWithPlayer in rollsWithPlayer.Where(w => w.Rolls != null && w.Rolls.Count > 0))
            {
                if (rollWithPlayer.CharacterSheetInstanceId == persona.CharacterSheetInstanceId)
                {
                    foreach (var specificRoll in rollWithPlayer.Rolls)
                    {
                        str.AppendLine("<roll>");
                        str.AppendLine($"{{{{user}}}} has rolled {specificRoll.Value} for the category {specificRoll.ActionCategory}.");

                        str.AppendLine("<otherCharactersInScene>");
                        foreach (var charInScene in specificRoll.CharactersInScene.Where(w=>w.CharacterInSceneCounterRoll?.Value != null))
                        {
                            str.AppendLine($"{charInScene.CharacterName} rolled {charInScene.CharacterInSceneCounterRoll.Value} against {{{{user}}}} attempt.");
                        }

                        str.AppendLine("</otherCharactersInScene>");

                        if (specificRoll.Guides != null && specificRoll.Guides.Any())
                        {
                            foreach (var guide in specificRoll.Guides)
                            {
                                str.AppendLine("<guidance>");
                                str.AppendLine($"<reasoning>{guide.Reasoning}</reasoning>");
                                str.AppendLine($"<reactionFromOtherCharactersWhenSucceedingSkillCheck>{guide.ReactionFromOtherCharactersWhenSucceedingSkillCheck}</reactionFromOtherCharactersWhenSucceedingSkillCheck>");
                                str.AppendLine($"<reactionFromOtherCharactersWhenFailingSkillCheck>{guide.ReactionFromOtherCharactersWhenFailingSkillCheck}</reactionFromOtherCharactersWhenFailingSkillCheck>");
                                str.AppendLine("</guidance>");
                            }
                        }

                        str.AppendLine("</roll>");
                    }
                } else
                {
                    foreach (var specificRoll in rollWithPlayer.Rolls)
                    {
                        var characterSrc = chatCharacterSheetInstance?.CharacterSheetInstances?.FirstOrDefault(f => f.CharacterSheetInstanceId == rollWithPlayer.CharacterSheetInstanceId);
                        if (characterSrc?.CharacterSheet == null)
                        {
                            continue;
                        }

                        var characterNameInScene = GetCharacterFullName(characterSrc.CharacterSheet?.FirstName, characterSrc.CharacterSheet.LastName);
                        str.AppendLine("<roll>");
                        str.AppendLine($"{characterNameInScene} has rolled {specificRoll.Value} for the category {specificRoll.ActionCategory}.");

                        var playerInScene = specificRoll.CharactersInScene.FirstOrDefault(f => f.CharacterSheetInstanceId == persona.CharacterSheetInstanceId);
                        if (playerInScene?.CharacterInSceneCounterRoll?.Value != null)
                        {
                            str.AppendLine("<player({{user}})>");
                            str.AppendLine($"{{{{user}}}} rolled {playerInScene.CharacterInSceneCounterRoll.Value} against {characterNameInScene} attempt.");
                            str.AppendLine("</player({{user}})>");
                        }

                        if (specificRoll.Guides != null && specificRoll.Guides.Any())
                        {
                            foreach (var guide in specificRoll.Guides)
                            {
                                str.AppendLine("<guidance>");
                                str.AppendLine($"<reasoning>{guide.Reasoning}</reasoning>");
                                str.AppendLine($"<reactionFromOtherCharactersWhenSucceedingSkillCheck>{guide.ReactionFromOtherCharactersWhenSucceedingSkillCheck}</reactionFromOtherCharactersWhenSucceedingSkillCheck>");
                                str.AppendLine($"<reactionFromOtherCharactersWhenFailingSkillCheck>{guide.ReactionFromOtherCharactersWhenFailingSkillCheck}</reactionFromOtherCharactersWhenFailingSkillCheck>");
                                str.AppendLine("</guidance>");
                            }
                        }

                        str.AppendLine("</roll>");
                    }
                }
            }
            str.AppendLine("</rolls>");

            return ($"<story_scene>{Environment.NewLine}{promptContextFormatElement?.Options?.Format?
                .Replace("{{messages_for_context_on_scene}}", contextOnScene)
                .Replace("{{rolls}}", str.ToString())
                .Replace("{{scene}}", lastMessage)}{Environment.NewLine}</story_scene>"
                .InjectMacros(personaLinkedToChat?.Name, null),
                new ShareableContextLink
                {
                    LinkedBuilder = this,
                });
        }

        private static string GetCharacterFullName(string firstName, string lastName)
        {
            string name = firstName?.Trim();
            if (!string.IsNullOrWhiteSpace(lastName))
            {
                name = $"{firstName} {lastName}".Trim();
            }
            return name;
        }
    }
}
