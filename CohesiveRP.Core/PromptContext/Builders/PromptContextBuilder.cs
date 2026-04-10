using System.Text;
using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Core.PromptContext.Abstractions;
using CohesiveRP.Core.PromptContext.Builders;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.Services.LLMApiProvider.OpenAI.BusinessObjects.Request;
using CohesiveRP.Storage.DataAccessLayer.AIQueries;
using CohesiveRP.Storage.DataAccessLayer.BackgroundQueries.BusinessObjects;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Format;
using CohesiveRP.Storage.DataAccessLayer.Settings;
using CohesiveRP.Storage.QueryModels.Chat;

namespace CohesiveRP.Core.PromptContext.Summary
{
    public class PromptContextBuilder : IPromptContextBuilder
    {
        private IPromptContextElementBuilderFactory promptContextElementBuilderFactory;
        private IStorageService storageService;
        private ChatCompletionPresetType presetType;
        private GlobalSettingsDbModel settings;
        private BackgroundQueryDbModel backgroundQuery;
        public BackgroundQuerySystemTags tag;
        private ChatCompletionPresetsDbModel presetTypeChatCompletionPreset;

        public ChatCompletionPresetsDbModel GetChatCompletionPreset() => presetTypeChatCompletionPreset;

        public PromptContextBuilder(ChatCompletionPresetType presetType, IPromptContextElementBuilderFactory promptContextElementBuilderFactory, IStorageService storageService, GlobalSettingsDbModel settings, BackgroundQueryDbModel backgroundQuery, BackgroundQuerySystemTags tag)
        {
            this.promptContextElementBuilderFactory = promptContextElementBuilderFactory;
            this.storageService = storageService;
            this.presetType = presetType;
            this.settings = settings;
            this.backgroundQuery = backgroundQuery;
            this.tag = tag;
        }

        public virtual async Task<IPromptContext> BuildAsync(string chatId)
        {
            try
            {
                // build the prompt context from the Db config, associate context for summarization, etc
                // Start by getting the chat so we know which presetType ChatCompletionPreset it maps to
                var chat = await storageService.GetChatAsync(chatId);
                if (chat == null)
                {
                    LoggingManager.LogToFile("9472f0e2-0f85-4134-810a-bff77443d9f7", $"Can't build the [{presetType}] prompt context for chat [{chatId}]. Chat was not found in storage.");
                    return null;
                }

                string chatCompletionPresetId = chat.SelectedChatCompletionPresets.FirstOrDefault(f => f.Type == presetType)?.ChatCompletionPresetId;

                if (chatCompletionPresetId == null)
                {
                    LoggingManager.LogToFile("29dd728a-185a-4ede-9625-d877ebf0f0f6", $"Can't build the [{presetType}] prompt context for chat [{chatId}]. That chat didn't map to a [{presetType}] ChatCompletionPresetType. A ChatCompletionPreset of this type is required to build the {presetType} context.");
                    return null;
                }

                // Now we need the ChatCompletionPreset mapped to the presetType promptContext configuration of that chat
                presetTypeChatCompletionPreset = await storageService.GetChatCompletionPresetAsync(chatCompletionPresetId);

                if (presetTypeChatCompletionPreset?.Format?.OrderedElementsWithinTheGlobalPromptContext == null)
                {
                    LoggingManager.LogToFile("f2a19231-d573-4849-a936-046c2269e88f", $"Can't build the [{presetType}] prompt context for chat [{chatId}]. [{presetType}] ChatCompletionPreset [{chatCompletionPresetId}] was not found in storage.");
                    return null;
                }

                var personaLinkedToChat = await storageService.GetPersonaByIdAsync(chat.PersonaId);
                var charactersLinkedToChat = await storageService.GetCharactersAsync();
                charactersLinkedToChat = charactersLinkedToChat.Where(w => chat.CharacterIds.Any(a => a == w.CharacterId)).ToArray();

                // for each elements in our format, we need to build them accordingly
                List<IShareableContextLink> shareableLinks = new();
                StringBuilder str = new();
                foreach (PromptContextFormatElement contextElement in presetTypeChatCompletionPreset.Format.OrderedElementsWithinTheGlobalPromptContext.Where(w => w != null && w.Enabled))
                {
                    var builder = await promptContextElementBuilderFactory.GenerateBuilderAsync(contextElement, settings, chat, backgroundQuery, tag, personaLinkedToChat, charactersLinkedToChat);

                    if (builder == null)
                    {
                        LoggingManager.LogToFile("639b1eea-5985-4a42-9e08-d951c5583c1b", $"There was no builder from factory for [{contextElement.Tag}] tag context element.");
                        continue;
                    }

                    (string result, IShareableContextLink link) = await builder.BuildAsync();
                    str.Append(result);
                    shareableLinks.Add(link);
                }

                return new PromptContext
                {
                    Value = str.ToString(),
                    Messages =
                    [
                        new OpenAIChatCompletionMessage
                    {
                        Role = OpenAIChatCompletionRole.system,
                        Content = str.ToString(),
                    }
                    ],
                    ShareableContextLinks = shareableLinks,
                };
            } catch (Exception e)
            {
                LoggingManager.LogToFile("470f5794-e202-4dfe-b515-9574843f299b", $"Unhandled error whilst generating PromptContext.", e);
                throw;
            }
        }
    }
}
