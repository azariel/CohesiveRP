using System.Text;
using CohesiveRP.Core.PromptContext.Abstractions;
using CohesiveRP.Core.Services;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Format;
using CohesiveRP.Storage.DataAccessLayer.Chats;

namespace CohesiveRP.Core.PromptContext.Builders.Directive
{
    public class PromptContextDirectCharactersInjectionBuilder : IPromptContextElementBuilder
    {
        private IStorageService storageService;
        private PromptContextFormatElement promptContextFormatElement;
        private ChatDbModel chatDbModel;

        public PromptContextDirectCharactersInjectionBuilder(IStorageService storageService, PromptContextFormatElement promptContextFormatElement, ChatDbModel chatDbModel)
        {
            this.storageService = storageService;
            this.promptContextFormatElement = promptContextFormatElement;
            this.chatDbModel = chatDbModel;
        }

        public async Task<(string, IShareableContextLink)> BuildAsync()
        {
            if(chatDbModel?.CharacterIds == null || chatDbModel.CharacterIds.Count <= 0)
            {
                return (null, new ShareableContextLink{ LinkedBuilder = this });
            }

            var characters = await storageService.GetCharactersAsync();
            string str = "";
            foreach (var characterId in chatDbModel.CharacterIds)
            {
                var character = characters.FirstOrDefault(f => f.CharacterId == characterId);

                if(!string.IsNullOrWhiteSpace(character?.Description))
                {
                    str += promptContextFormatElement?.Options?.Format?
                        .Replace("{{character_name}}", character.Name)
                        .Replace("{{character_description}}", character.Description)
                        .Replace(Constants.CHARACTER_PLACEHOLDER, character.Name);
                }
            }
            
            return ($"# Characters {Environment.NewLine}{str}", new ShareableContextLink { LinkedBuilder = this });
        }
    }
}
