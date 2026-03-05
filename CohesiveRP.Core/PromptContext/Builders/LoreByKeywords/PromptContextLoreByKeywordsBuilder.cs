using System.Text;
using CohesiveRP.Core.Services;
using CohesiveRP.Storage.DataAccessLayer.ChatCompletionPresets.BusinessObjects.Format;

namespace CohesiveRP.Core.PromptContext.Builders.Directive
{
    public class PromptContextLoreByKeywordsBuilder : IPromptContextElementBuilder
    {
        public PromptContextLoreByKeywordsBuilder(IStorageService storageService)
        {

        }

        public async Task<string> BuildAsync(PromptContextFormatElement promptContextFormatElement)
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

            return $"# Lore{Environment.NewLine}{str}";
        }
    }
}
