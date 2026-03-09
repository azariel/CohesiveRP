using System.Text;
using CohesiveRP.Core.PromptContext.Abstractions;
using CohesiveRP.Core.Services;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Format;
using CohesiveRP.Storage.DataAccessLayer.Chats;

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
            Dictionary<string, string> loreByKeywordsContent = [];
            string userPersonaName = "userName";// TODO: fetch from Db

            //TODO: fetch from Db
            //loreByKeywordsContent.Add("loreDummyKey1", "loreDummyValue1");
            //loreByKeywordsContent.Add("loreDummyKey2", "loreDummyValue2");
            //loreByKeywordsContent.Add("loreDummyKey3", "loreDummyValue3");

            StringBuilder str = new();
            foreach (var loreKeywordContent in loreByKeywordsContent)
            {
                string value = promptContextFormatElement?.Options?.Format?
                    .Replace("{{item_header}}", loreKeywordContent.Key)
                    .Replace("{{item_description}}", loreKeywordContent.Key)
                    .Replace("{{user}}", userPersonaName);

                if (value != null)
                {
                    str.Append(value);
                }
            }

            if (loreByKeywordsContent.Count <= 0)
            {
                str.Append($"Infer the lore from the roleplay context.{Environment.NewLine}{Environment.NewLine}");
            }

            return ($"# Lore{Environment.NewLine}{str}", new ShareableContextLink{ LinkedBuilder = this });
        }
    }
}
