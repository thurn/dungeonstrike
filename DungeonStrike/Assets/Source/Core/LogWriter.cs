using System;
using UnityEngine;
using System.IO;
using System.Text;

namespace DungeonStrike.Source.Core
{
    public static class LogWriter
    {
        private static readonly StreamWriter LogFile;
        private const string MetadataSeparator = "\t\t»";

        static LogWriter()
        {
            var logDir = Path.Combine(Application.dataPath, "Logs");
            Directory.CreateDirectory(logDir);
            var logFile = Path.Combine(logDir, "client_logs.txt");
            LogFile = new StreamWriter(logFile, true /* append */) {AutoFlush = true};
        }

        /// <summary>
        /// Handler for Unity log callbacks. See <see cref="Application.logMessageReceivedThreaded"/>.
        /// </summary>
        public static void HandleUnityLog(string logString, string stackTrace, LogType logType)
        {
            var result = new StringBuilder(logString);
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

            result.Append(StackTraceMetadata(stackTrace));
            WriteLog(result);
        }

        /// <summary>
        /// Creates a string builder containing the first part of a log metadata entry.
        /// </summary>
        /// <param name="logContext">The log context for this metadata.</param>
        /// <returns>A log metadata entry with the associated context and current timestamp.</returns>
        public static StringBuilder StartLogMetadata(LogContext logContext)
        {
            var result = new StringBuilder();
            result.Append(MetadataSeparator);
            result.Append("{");
            logContext.AppendContextParameters(result);
            AppendTimestampAndType(result);
            return result;
        }

        private static void AppendTimestampAndType(StringBuilder builder)
        {
            var unixTimestamp = (long) DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
            builder.Append(":timestamp ").Append(unixTimestamp).Append(", ");
            builder.Append(":log-type :client, ");
        }

        private static StringBuilder StackTraceMetadata(string stackTrace)
        {
            var result = new StringBuilder();
            result.Append(":stack-trace \"").Append(stackTrace.Replace("\n", "\\n")).Append("\"}\n");
            return result;
        }

        /// <summary>
        /// Writes a new entry to the log file.
        /// </summary>
        /// <param name="message">Log message, including log metadata.</param>
        private static void WriteLog(StringBuilder message)
        {
            LogFile.Write(message.ToString());
        }
    }
}
