using System.Text;
using CohesiveRP.Core.PromptContext.Format;
using CohesiveRP.Core.Services;

namespace CohesiveRP.Core.PromptContext.Builders.Directive
{
    public class PromptContextLoreByQueryBuilder : IPromptContextElementBuilder
    {
        public PromptContextLoreByQueryBuilder(IStorageService storageService)
        {
            
        }

        public async Task<string> BuildAsync(PromptContextFormatElement promptContextFormatElement)
        {
            Dictionary<string, string> loreByQueryContent = [];
            string userPersonaName = "userName";// TODO: fetch from Db

            //TODO: fetch from Db
            //loreByQueryContent.Add("le chateau de montenac", "c'est un beau château");
            //loreByQueryContent.Add("loreByQueryContentKey2", "loreByQueryContentValue2");
            //loreByQueryContent.Add("loreByQueryContentKey3", "loreByQueryContentValue3");

            StringBuilder str = new();
            foreach (var loreQueryContent in loreByQueryContent)
            {
                string value = promptContextFormatElement?.Options?.Format?
                    .Replace("{{item_header}}", loreQueryContent.Key)
                    .Replace("{{item_description}}", loreQueryContent.Key)
                    .Replace("{{user}}", userPersonaName);

                if(value != null)
                {
                    str.Append(value);
                }
            }

            if (loreByQueryContent.Count <= 0)
            {
                str.Append($"Infer the lore from the roleplay context.{Environment.NewLine}{Environment.NewLine}");
            }

            return $"# Lore (relevant in recent story context){Environment.NewLine}{str}";
        }
    }
}
