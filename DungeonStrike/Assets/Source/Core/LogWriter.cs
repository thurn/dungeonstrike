using System;
using UnityEngine;
using System.IO;
using System.Text;

namespace DungeonStrike.Source.Core
{
    public static class LogWriter
    {
        public static string LogFilePath { get; private set; }
        private static StreamWriter _logFile;
        private static bool _disabled;
        private const string MetadataSeparator = "\t\t»";

        /// <summary>
        /// Initialize the log file. Must be invoked at startup. In order to avoid logging during unit test execution,
        /// this method should not be called during tests. This method is safe to call multiple times.
        /// </summary>
        public static void Initialize()
        {
            if (_disabled || (_logFile != null)) return;
            var logDir = Path.Combine(Application.dataPath, "../Logs");
            Directory.CreateDirectory(logDir);
            LogFilePath = Path.Combine(logDir, "client_logs.txt");
            _logFile = new StreamWriter(LogFilePath, true /* append */) {AutoFlush = true};
            Application.logMessageReceivedThreaded += HandleUnityLog;
        }

        public static void DisableForTests()
        {
            _disabled = true;
        }

        /// <summary>
        /// Handler for Unity log callbacks. See <see cref="Application.logMessageReceivedThreaded"/>.
        /// </summary>
        public static void HandleUnityLog(string logString, string stackTrace, LogType logType)
        {
            if ((logType == LogType.Warning) &&
                (logString.Contains("Assets/ThirdParty") || logString.Contains("Assets/OldSource")))
            {
                // Ignore warnings in third-party code.
                return;
            }

            var result = new StringBuilder(logString.Replace("\n", " \\n "));
            if (!logString.Contains(MetadataSeparator))
            {
                result.Append(MetadataSeparator);
                result.Append("{");
                result.Append(":source \"").Append("Unity" + logType).Append("\", ");
                if (logType != LogType.Log)
                {
                    result.Append(":error? true, ");
                }
                AppendTimestampAndType(result);
            }

            AppendStackTraceMetadata(result, stackTrace);
            WriteLog(result);
        }

        /// <summary>
        /// Prepares a string for logging output via Debug.Log or an exception message. The resulting string will
        /// contain the *start* of a log metadata entry, but not the *end* of that entry, so that the metadata can be
        /// expanded by the log handler in <see cref="HandleUnityLog"/> above.
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="logContext">Log context for message</param>
        /// <param name="error">True if this should be considered an error</param>
        /// <param name="arguments">Additional arguments to append to the log message. With two or more values, will
        /// be formatted in key=value format.</param>
        /// <returns>String appropriate for output to the unity logs.</returns>
        public static string FormatForLogOutput(string message, LogContext logContext, bool error, object[] arguments)
        {
            var result = new StringBuilder(message.Replace("\n", " \\n "));
            if (arguments.Length == 1)
            {
                result.Append(" [").Append(arguments[0]).Append("]");
            }
            else if (arguments.Length > 1)
            {
                result.Append(" [");
                for (var i = 0; i < arguments.Length; ++i)
                {
                    result.Append(arguments[i]);
                    result.Append(i % 2 == 0 ? "=" : ", ");
                }
                result.Append("]");
            }
            result.Append(StartLogMetadata(logContext, false));
            return result.ToString();
        }

        /// <summary>
        /// Creates a string builder containing the first part of a log metadata entry.
        /// </summary>
        /// <param name="logContext">The log context for this metadata.</param>
        /// <param name="error">True to indicate that this entry should be considered an error.</param>
        /// <returns>A log metadata entry with the associated context and current timestamp.</returns>
        private static StringBuilder StartLogMetadata(LogContext logContext, bool error)
        {
            var result = new StringBuilder();
            result.Append(MetadataSeparator);
            result.Append("{");
            logContext.AppendContextParameters(result);
            if (error)
            {
                result.Append(":error? true, ");
            }
            AppendTimestampAndType(result);
            return result;
        }

        /// <summary>
        /// Adds a timestamp and log type entry to the partial log in "builder".
        /// </summary>
        private static void AppendTimestampAndType(StringBuilder builder)
        {
            var unixTimestamp = (long) DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
            builder.Append(":timestamp ").Append(unixTimestamp).Append(", ");
            builder.Append(":log-type :client, ");
        }

        /// <summary>
        /// Appends a "stackTrace" to the partial log entry in "result".
        /// </summary>
        private static void AppendStackTraceMetadata(StringBuilder result, string stackTrace)
        {
            result.Append(":stack-trace \"").Append(stackTrace.Replace("\n", "\\n")).Append("\"}\n");
        }

        /// <summary>
        /// Writes a new entry to the log file.
        /// </summary>
        /// <param name="message">Log message, including log metadata.</param>
        private static void WriteLog(StringBuilder message)
        {
            if (!_disabled)
            {
                _logFile.Write(message.ToString());
            }
        }
    }
}