namespace CohesiveRP.Common.Configuration
{
    public class ComfyUiEndpointConfig
    {
        public string BaseUrl { get; set; } = "http://192.168.0.237:8188";
        public int ExecutionTimeoutMinutes { get; set; } = 10;
    }
}
