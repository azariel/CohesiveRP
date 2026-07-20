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

        private static readonly object _RollLock = new object();
        private const long LOG_ROLL_THRESHOLD_BYTES = 15 * 1024 * 1024;

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

            _Message = $"{DateTime.UtcNow:yyyy-MM-dd HH.mm.ss.fff} - [{logUID}] {_Message}";

            try
            {
                if (!Directory.Exists(Path.GetDirectoryName(logFilePath)))
                    Directory.CreateDirectory(Path.GetDirectoryName(logFilePath));
            }
            catch (Exception)
            {
                // Best effort
            }

            int retryDecrementor = 100;
            while (retryDecrementor > 0)
            {
                try
                {
                    File.AppendAllText(logFilePath, _Message);
                    RollLogFileIfNeeded(logFilePath);
                    return;
                }
                catch (Exception)
                {
                    Thread.Sleep(50);
                }

                --retryDecrementor;
            }
        }

        // ********************************************************************
        //                            Private
        // ********************************************************************
        private static void RollLogFileIfNeeded(string logFilePath)
        {
            var fileInfo = new FileInfo(logFilePath);
            if (!fileInfo.Exists || fileInfo.Length < LOG_ROLL_THRESHOLD_BYTES)
                return;

            // Non-blocking: if another thread is already rolling, skip — it will handle it
            if (!Monitor.TryEnter(_RollLock))
                return;

            try
            {
                // Re-check inside the lock: a concurrent roll may have just finished
                fileInfo.Refresh();
                if (fileInfo.Length < LOG_ROLL_THRESHOLD_BYTES)
                    return;

                string[] lines = File.ReadAllLines(logFilePath);
                int trimCount = lines.Length / 3;
                string rollMarker = $"{DateTime.UtcNow:yyyy-MM-dd HH.mm.ss.fff} - [LogRoll] Oldest 1/3 trimmed ({trimCount} lines removed)";

                File.WriteAllLines(logFilePath, new[] { rollMarker }.Concat(lines.Skip(trimCount)));
            }
            catch (Exception)
            {
                // Best effort
            }
            finally
            {
                Monitor.Exit(_RollLock);
            }
        }
    }
}