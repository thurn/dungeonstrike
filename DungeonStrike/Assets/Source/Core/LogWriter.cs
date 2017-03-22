using System;
using UnityEngine;
using System.IO;
using System.Text;

namespace DungeonStrike.Source.Core
{
    public static class LogWriter
    {
        private static readonly StreamWriter LogFile;

        static LogWriter()
        {
            var logDir = Path.Combine(Application.dataPath, "Logs");
            Directory.CreateDirectory(logDir);
            var logFile = Path.Combine(logDir, "logs.txt");
            LogFile = new StreamWriter(logFile, false /* append */) {AutoFlush = true};
        }

        /// <summary>
        /// Handler for Unity log callbacks. See <see cref="Application.logMessageReceivedThreaded"/>.
        /// </summary>
        public static void HandleUnityLog(string logString, string stackTrace, LogType logType)
        {
            if (logString.StartsWith("DSLOG"))
            {
                WriteLog(new StringBuilder(logString), stackTrace);
            }
            else
            {
                var header = CreateLogHeader("Unity" + logType);
                header.Append(logString);
                WriteLog(header, stackTrace);
            }
        }

        /// <summary>
        /// Creates a new DSLOG header.
        /// </summary>
        /// <param name="logType">The log type for this header.</param>
        /// <returns>A log header with the associated header and current timestamp.</returns>
        public static StringBuilder CreateLogHeader(string logType)
        {
            var result = new StringBuilder();
            result.Append("DSLOG[");
            var unixTimestamp = (long) DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
            result.Append(unixTimestamp);
            result.Append("][").Append(logType).Append("]\n");
            return result;
        }

        /// <summary>
        /// Writes a new entry to the log file.
        /// </summary>
        /// <param name="message">Log message, must include log header.</param>
        /// <param name="stackTrace">Stack trace for code creating this log entry.</param>
        private static void WriteLog(StringBuilder message, string stackTrace)
        {
            message.Append("\nDSTRACE\n");
            message.Append(stackTrace);
            message.Append("ENDLOG\n\n");
            LogFile.Write(message.ToString());
        }
    }
}
