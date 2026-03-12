using System.Text;
using CohesiveRP.Core.PromptContext.Abstractions;
using CohesiveRP.Core.Services;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Format;
using CohesiveRP.Storage.DataAccessLayer.Chats;

namespace CohesiveRP.Core.PromptContext.Builders.Directive
{
    public class PromptContextRelevantCharactersBuilder : IPromptContextElementBuilder
    {
        private IStorageService storageService;
        private PromptContextFormatElement promptContextFormatElement;
        private ChatDbModel chatDbModel;

        public PromptContextRelevantCharactersBuilder(IStorageService storageService, PromptContextFormatElement promptContextFormatElement, ChatDbModel chatDbModel)
        {
            this.storageService = storageService;
            this.promptContextFormatElement = promptContextFormatElement;
            this.chatDbModel = chatDbModel;
        }

        public async Task<(string, IShareableContextLink)> BuildAsync()
        {
            Dictionary<string, string> loreByQueryContent = [];
            string userPersonaName = "userName";// TODO: fetch from Db

            //TODO: fetch from Db
            //loreByQueryContent.Add("myCharacter1", "bunch of details");
            //loreByQueryContent.Add("myCharacter2", "bunch of details 2");

            StringBuilder str = new();
            foreach (var loreQueryContent in loreByQueryContent)
            {
                string value = promptContextFormatElement?.Options?.Format?
                    .Replace("{{item_header}}", loreQueryContent.Key)
                    .Replace("{{item_description}}", loreQueryContent.Key)
                    .Replace(Constants.USER_PLACEHOLDER, userPersonaName);

                if(value != null)
                {
                    str.Append(value);
                }
            }

            if (loreByQueryContent.Count <= 0)
            {
                str.Append($"Infer the relevant characters from the roleplay context.{Environment.NewLine}{Environment.NewLine}");
            }

            return ($"# Relevant Characters{Environment.NewLine}{str}", new ShareableContextLink { LinkedBuilder = this });
        }
    }
}
