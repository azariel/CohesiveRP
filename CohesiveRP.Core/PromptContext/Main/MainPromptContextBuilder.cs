using System.Text;
using CohesiveRP.Common.Diagnostics;
using CohesiveRP.Common.Serialization;
using CohesiveRP.Core.PromptContext.Abstractions;
using CohesiveRP.Core.PromptContext.Builders;
using CohesiveRP.Core.PromptContext.Format;
using CohesiveRP.Storage.DataAccessLayer.AIQueries;

namespace CohesiveRP.Core.PromptContext.Main
{
    public class MainPromptContextBuilder : IPromptContextBuilder
    {
        private IPromptContextElementBuilderFactory promptContextElementBuilderFactory;

        public MainPromptContextBuilder(IPromptContextElementBuilderFactory promptContextElementBuilderFactory)
        {
            this.promptContextElementBuilderFactory = promptContextElementBuilderFactory;
        }

        public async Task<IPromptContext> BuildAsync(string chatId)
        {
            // build the prompt context from the Db config, associate characters, dynamic memory, yada yada
            // TODO: get the GlobalPromptContextFormat model from Db. ChatCompletionPreset.PromptContextFormat

            GlobalPromptContextFormat promptContextFormat = await servideDal.GetChatCompletionPresetByChatId(chatId);

            // For Debug
            //GlobalPromptContextFormat globalPromptContextFormat = new GlobalPromptContextFormat
            //{
            //    OrderedElementsWithinTheGlobalPromptContext =
            //    [
            //        new(){ Tag = PromptContextFormatTag.Directive, Options = new() { Format = $"{{{{item_description}}}}{Environment.NewLine}{Environment.NewLine}" } },
            //        new(){ Tag = PromptContextFormatTag.World, Options = new() { Format = $"{{{{item_description}}}}{Environment.NewLine}{Environment.NewLine}" } },
            //        new(){ Tag = PromptContextFormatTag.LoreByKeywords, Options = new() { Format = $"## {{{{item_header}}}}{Environment.NewLine}{{{{item_description}}}}{Environment.NewLine}{Environment.NewLine}" } },
            //        new(){ Tag = PromptContextFormatTag.SummaryExtraTerm, Options = new() { Format = $"- {{{{item_description}}}}{Environment.NewLine}{Environment.NewLine}" } },
            //        new(){ Tag = PromptContextFormatTag.SummaryLongTerm, Options = new() { Format = $"- {{{{item_description}}}}{Environment.NewLine}{Environment.NewLine}" } },
            //        new(){ Tag = PromptContextFormatTag.SummaryMediumTerm, Options = new() { Format = $"- {{{{item_description}}}}{Environment.NewLine}{Environment.NewLine}" } },
            //        new(){ Tag = PromptContextFormatTag.SummaryShortTerm, Options = new() { Format = $"- {{{{item_description}}}}{Environment.NewLine}{Environment.NewLine}" } },
            //        new(){ Tag = PromptContextFormatTag.LoreByQuery, Options = new() { Format = $"## {{{{item_header}}}}{Environment.NewLine}{{{{item_description}}}}{Environment.NewLine}{Environment.NewLine}" } },
            //        new(){ Tag = PromptContextFormatTag.RelevantCharacters, Options = new() { Format = $"## {{{{item_header}}}}{Environment.NewLine}{{{{item_description}}}}{Environment.NewLine}{Environment.NewLine}" } },
            //        new(){ Tag = PromptContextFormatTag.LastXMessages, Options = new() { Format = $"{{{{item_description}}}}{Environment.NewLine}{Environment.NewLine}" } },
            //        new(){ Tag = PromptContextFormatTag.SceneTracker, Options = new() { Format = $"{{{{item_description}}}}{Environment.NewLine}{Environment.NewLine}" } },
            //        new(){ Tag = PromptContextFormatTag.CurrentObjective, Options = new() { Format = $"{{{{item_description}}}}{Environment.NewLine}{Environment.NewLine}" } },
            //        new(){ Tag = PromptContextFormatTag.LastUserMessage, Options = new() { Format = $"{{{{item_description}}}}{Environment.NewLine}{Environment.NewLine}" } },
            //        new(){ Tag = PromptContextFormatTag.BehavioralInstructions, Options = new() { Format = $"{{{{item_description}}}}{Environment.NewLine}{Environment.NewLine}" } },
            //    ]
            //};

            //string a = JsonCommonSerializer.SerializeToString(globalPromptContextFormat);

            // for each elements in our format, we need to build them accordingly
            StringBuilder str = new();
            foreach (var contextElement in promptContextFormat.OrderedElementsWithinTheGlobalPromptContext)
            {
                var builder = await promptContextElementBuilderFactory.GenerateBuilderAsync(contextElement);

                if(builder == null)
                {
                    LoggingManager.LogToFile("f9705042-ef1d-42e2-ad89-1c0070a4c9f1", $"There was no builder from factory for [{contextElement.Tag}] tag context element.");
                    continue;
                }

                var result = await builder.BuildAsync(contextElement);
                str.Append(result);
            }

            // For debug
            return new PromptContext
            {
                Value = str.ToString(),
            };
        }
    }
}
