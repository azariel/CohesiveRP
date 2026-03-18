using System.Text;
using CohesiveRP.Core.PromptContext.Abstractions;
using CohesiveRP.Core.Services;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Format;
using CohesiveRP.Storage.DataAccessLayer.Chats;
using CohesiveRP.Storage.DataAccessLayer.Lorebooks.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.Messages;
using CohesiveRP.Storage.DataAccessLayer.Messages.Hot;

namespace CohesiveRP.Core.PromptContext.Builders.Directive
{
    public class PromptContextLoreByKeywordsBuilder : IPromptContextElementBuilder
    {
        private IStorageService storageService;
        private PromptContextFormatElement promptContextFormatElement;
        private ChatDbModel chatDbModel;

        public PromptContextLoreByKeywordsBuilder(IStorageService storageService, PromptContextFormatElement promptContextFormatElement, ChatDbModel chatDbModel)
        {
            this.storageService = storageService;
            this.promptContextFormatElement = promptContextFormatElement;
            this.chatDbModel = chatDbModel;
        }

        public async Task<(string, IShareableContextLink)> BuildAsync()
        {
            ChatDbModel chat = await storageService.GetChatAsync(chatDbModel?.ChatId);

            if (chat?.LorebookIds == null || chat.LorebookIds.Count <= 0)
            {
                return (null, new ShareableContextLink { LinkedBuilder = this });
            }

            PersonaDbModel defaultPersona = await storageService.GetPersonaByIdAsync(chatDbModel?.PersonaId);
            string userPersonaName = defaultPersona.Name;

            // Fetch the lorebooks tethered to this chat
            var lorebooks = await storageService.GetLorebooksAsync(f => chat.LorebookIds.Contains(f.LorebookId));
            if (lorebooks == null || lorebooks.Length <= 0)
            {
                return (null, new ShareableContextLink { LinkedBuilder = this });
            }

            HotMessagesDbModel hotMessages = await storageService.GetAllHotMessagesAsync(chat.ChatId);
            IOrderedEnumerable<MessageDbModel> orderedMessages = hotMessages.Messages.OrderByDescending(o => o.CreatedAtUtc);

            StringBuilder str = new();
            foreach (LorebookDbModel lorebook in lorebooks)
            {
                foreach (LorebookEntry entry in lorebook.Entries)
                {
                    if (!EvaluateIfLorebookKeyTriggers(entry, orderedMessages))
                    {
                        continue;
                    }

                    string value = promptContextFormatElement?.Options?.Format?
                        .Replace("{{item_header}}", entry.Name)
                        .Replace("{{item_description}}", entry.Content)
                        .Replace(Constants.USER_PLACEHOLDER, userPersonaName);

                    if (value != null)
                    {
                        str.Append(value);
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(str.ToString()))
            {
                str.Append($"Infer the lore from the roleplay context.{Environment.NewLine}{Environment.NewLine}");
            }

            return ($"# Lore{Environment.NewLine}{str}", new ShareableContextLink { LinkedBuilder = this });
        }

        /// <summary>
        /// Only inject the key/value when relevant in context.
        /// </summary>
        private bool EvaluateIfLorebookKeyTriggers(LorebookEntry entry, IOrderedEnumerable<MessageDbModel> messages)
        {
            if (entry.Depth <= 0)
            {
                return false;
            }

            List<MessageDbModel> messagesToProcess = messages.Take(entry.Depth).ToList();

            var stringComparison = StringComparison.InvariantCulture;
            if (entry.CaseSensitive)
            {
                stringComparison = StringComparison.InvariantCultureIgnoreCase;
            }

            foreach (MessageDbModel message in messagesToProcess)
            {
                if (entry.Keys.Any(a => message.Content.Contains(a, stringComparison)))
                {
                    return true;
                }
            }


            return false;
        }
    }
}
