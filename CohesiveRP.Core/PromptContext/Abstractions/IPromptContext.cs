namespace CohesiveRP.Core.PromptContext.Abstractions
{
    public interface IPromptContext
    {
        string Value { get; set; }
        IPromptMessage[] Messages { get; set; }
    }
}
