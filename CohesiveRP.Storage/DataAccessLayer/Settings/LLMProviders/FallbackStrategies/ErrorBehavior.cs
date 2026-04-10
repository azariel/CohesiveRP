namespace CohesiveRP.Storage.DataAccessLayer.Settings.LLMProviders.FallbackStrategies
{
    public class ErrorBehavior
    {
        public int NbErrorsBeforeTimeout { get; set; } = 3;
        public int TimeoutInSeconds { get; set; } = 120;
    }
}
