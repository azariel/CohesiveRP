using System.Text.Json.Serialization;
using static CohesiveRP.Common.Diagnostics.LoggingManager;

namespace CohesiveRP.Common.Configuration
{
    /// <summary>
    /// Global configuration for Radiant
    /// </summary>
    public class CohesiveRPCommonConfig
    {
        // ********************************************************************
        //                            Properties
        // ********************************************************************

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public LogVerbosity LogVerbosity { get; set; } = LogVerbosity.Minimal;
    }
}
