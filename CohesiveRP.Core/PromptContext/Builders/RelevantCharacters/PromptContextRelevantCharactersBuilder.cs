using System.Text;
using CohesiveRP.Core.PromptContext.Format;
using CohesiveRP.Core.Services;

namespace CohesiveRP.Core.PromptContext.Builders.Directive
{
    public class PromptContextRelevantCharactersBuilder : IPromptContextElementBuilder
    {
        public PromptContextRelevantCharactersBuilder(IStorageService storageService)
        {
            
        }

        public async Task<string> BuildAsync(PromptContextFormatElement promptContextFormatElement)
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
                    .Replace("{{user}}", userPersonaName);

                if(value != null)
                {
                    str.Append(value);
                }
            }

            if (loreByQueryContent.Count <= 0)
            {
                str.Append($"Infer the relevant characters from the roleplay context.{Environment.NewLine}{Environment.NewLine}");
            }

            return $"# Relevant Characters{Environment.NewLine}{str}";
        }
    }
}
