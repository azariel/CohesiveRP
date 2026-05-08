namespace CohesiveRP.Common.Configuration
{
    public class ComfyUiEndpointConfig
    {
        public string BaseUrl { get; set; } = "http://localhost:8188";
        public int ExecutionTimeoutMinutes { get; set; } = 10;
    }
}
