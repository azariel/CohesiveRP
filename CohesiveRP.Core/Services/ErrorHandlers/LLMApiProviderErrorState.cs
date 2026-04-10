namespace CohesiveRP.Core.Services.ErrorHandlers
{
    public record LLMApiProviderErrorState
    {
        public string ProviderConfigId { get; set; }
        public int ErrorsBalance { get; set; }
        public DateTime? TimeoutUntilDateTimeUtc { get; set; } = null;
    }
}
