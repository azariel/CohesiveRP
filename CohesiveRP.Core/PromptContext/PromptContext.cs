using CohesiveRP.Core.PromptContext.Abstractions;

namespace CohesiveRP.Core.PromptContext
{
    public class PromptContext : IPromptContext
    {
        /// <summary>
        /// The actual text to add to the LLM prompt as a context.
        /// </summary>
        public string Value { get; set; }
    }
}
