namespace CohesiveRP.Storage.DataAccessLayer.Settings.LLMProviders.FallbackStrategies
{
    public class FallbackStrategy
    {
        public int ErrorsTreshold { get; set; } = 3;
        public string ProviderConfigId { get; set; } = null;
        public int ErrorsTresholdBelowXToAllowFallback { get; set; } = 3;
    }
}
