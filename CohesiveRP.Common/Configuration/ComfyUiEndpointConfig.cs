namespace CohesiveRP.Common.Configuration
{
    public class ComfyUiEndpointConfig
    {
        public string BaseUrl { get; set; } = "http://127.0.0.1:8188";
        public int ExecutionTimeoutMinutes { get; set; } = 10;
    }
}
