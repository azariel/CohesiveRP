using CohesiveRP.Common.Configuration;
using CohesiveRP.Common.Utils;

namespace CohesiveRP.Common.Diagnostics
{
    public static class LoggingManager
    {
        // ********************************************************************
        //                            Nested
        // ********************************************************************
        public enum LogVerbosity
        {
            Minimal,
            Verbose
        }

        // ********************************************************************
        //                            Constants
        // ********************************************************************
        public const string DEFAULT_LOG_FILE_RELATIVE_PATH = "CohesiveRP.log";
        public static readonly LogVerbosity fLogVerbosity;

        static LoggingManager()
        {
            var _Config = CommonConfigurationManager.GetConfigFromMemory();
            fLogVerbosity = _Config.LogVerbosity;
        }

        // ********************************************************************
        //                            Public
        // ********************************************************************
        /// <summary>
        /// Format message and then log it to specified or default file
        /// </summary>
        public static void LogToFile(string logUID, string logContent, Exception exception = null, LogVerbosity logVerbosity = LogVerbosity.Minimal, string logFilePath = DEFAULT_LOG_FILE_RELATIVE_PATH)
        {
            if (logVerbosity > fLogVerbosity)
                return;

            string _Message = $"Message=[{logContent}]{Environment.NewLine}";

            if (exception != null)
                _Message += $"Exception=[{Environment.NewLine}{ExceptionUtils.BuildExceptionAndInnerExceptionsMessage(exception)}]{Environment.NewLine}";

            // Format message to add useful information
            _Message = $"{DateTime.UtcNow:yyyy-MM-dd HH.mm.ss.fff} - [{logUID}] {_Message}";

            int retryDecrementor = 100;
            while (retryDecrementor > 0)
            {
                try
                {
                    File.AppendAllText(logFilePath, _Message);
                    return;
                } catch (Exception)
                {
                    Thread.Sleep(50);
                }

                --retryDecrementor;
            }
        }
    }
}
