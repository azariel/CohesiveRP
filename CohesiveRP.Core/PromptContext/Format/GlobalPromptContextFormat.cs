namespace CohesiveRP.Core.PromptContext.Format
{
    public class GlobalPromptContextFormat
    {
        /* May look something similar as:
         *  # Directive(system role) (1500 tokens)
            # world (500 tokens)
            # lore(by keywords, 8000 tokens)
            # summary (memory pyramid) (last 2000 messages, after the latest 200, summarized into 1000 tokens)
            # summary (memory pyramid) (last 200 messages, after the latest 25, summarized into 1000 tokens)
            # summary (memory pyramid) (last 25 messages, after the latest 5, summarized into 500 tokens)
            # lore by llm query (1500 tokens)
            # relevant(active) chars (3000 tokens)
            # last 5 messages (1000 tokens)
            # tracker (scene, characters, their body positions, clothes, hands, etc) (1500 tokens)
            # current objective (what is currently going on in the last 5 messages in a single short paragraph) (300 tokens)
            # last player message (200 tokens)
            # behavioral instruction (300 tokens) / think step by step
         * */
        public List<PromptContextFormatElement> OrderedElementsWithinTheGlobalPromptContext { get; set; } = new();
    }
}
