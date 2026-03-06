using System.Text;
using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Core.PromptContext.Abstractions;
using CohesiveRP.Core.PromptContext.Builders;
using CohesiveRP.Core.Services;
using CohesiveRP.Core.Services.LLMApiProvider.OpenAI.BusinessObjects.Request;
using CohesiveRP.Storage.DataAccessLayer.AIQueries;

namespace CohesiveRP.Core.PromptContext.Main
{
    public class MainPromptContextBuilder : IPromptContextBuilder
    {
        private IPromptContextElementBuilderFactory promptContextElementBuilderFactory;
        private IStorageService storageService;

        public MainPromptContextBuilder(IPromptContextElementBuilderFactory promptContextElementBuilderFactory, IStorageService storageService)
        {
            this.promptContextElementBuilderFactory = promptContextElementBuilderFactory;
            this.storageService = storageService;
        }

        public async Task<IPromptContext> BuildAsync(string chatId)
        {
            // build the prompt context from the Db config, associate characters, dynamic memory, yada yada
            // Start by getting the chat so we know which MAIN ChatCompletionPreset it maps to
            var chat = await storageService.GetChatAsync(chatId);
            if (chat == null)
            {
                LoggingManager.LogToFile("7f2d661a-807a-4c19-bfbf-924187fb62d1", $"Can't build the Main prompt context for chat [{chatId}]. Chat was not found in storage.");
                return null;
            }

            string mainChatCompletionPresetId = chat.SelectedChatCompletionPresets.FirstOrDefault(f => f.Type == Storage.QueryModels.Chat.ChatCompletionPresetType.Main)?.ChatCompletionPresetId;

            if (mainChatCompletionPresetId == null)
            {
                LoggingManager.LogToFile("8c336eb5-83c8-49ea-aea6-b98f1a8836fe", $"Can't build the Main prompt context for chat [{chatId}]. That chat didn't map to a [{Storage.QueryModels.Chat.ChatCompletionPresetType.Main}] ChatCompletionPresetType. A ChatCompletionPreset of this type is required to build the Main context.");
                return null;
            }

            // Now we need the ChatCompletionPreset mapped to the MAIN promptContext configuration of that chat
            ChatCompletionPresetsDbModel mainChatCompletionPreset = await storageService.GetChatCompletionPreset(mainChatCompletionPresetId);

            if (mainChatCompletionPreset?.Format == null)
            {
                LoggingManager.LogToFile("00ba38d5-50b2-45f3-b528-e3a2c45b8414", $"Can't build the Main prompt context for chat [{chatId}]. Main ChatCompletionPreset [{mainChatCompletionPresetId}] was not found in storage.");
                return null;
            }

            // for each elements in our format, we need to build them accordingly
            StringBuilder str = new();
            foreach (var contextElement in mainChatCompletionPreset.Format.OrderedElementsWithinTheGlobalPromptContext)
            {
                var builder = await promptContextElementBuilderFactory.GenerateBuilderAsync(contextElement, chat);

                if (builder == null)
                {
                    LoggingManager.LogToFile("f9705042-ef1d-42e2-ad89-1c0070a4c9f1", $"There was no builder from factory for [{contextElement.Tag}] tag context element.");
                    continue;
                }

                var result = await builder.BuildAsync();
                str.Append(result);
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
            };
        }
    }
}
