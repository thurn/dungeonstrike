using System.Text;
using UnityEngine;

namespace DungeonStrike.Source.Core
{
    /// <summary>
    /// Provides utility methods for logging component actions.
    /// </summary>
    /// <remarks>
    /// This class is used to implement component logging. Components should, in general, log any non-trivial transition
    /// in their internal state.
    /// </remarks>
    public sealed class Logger
    {
        private readonly LogContext _logContext;

        /// <summary>
        /// Create a new Logger.
        /// </summary>
        /// <param name="componentName">The name of the component which owns this Logger, included in logs.</param>
        public Logger(LogContext componentName)
        {
            _logContext = componentName;
        }

        /// <summary>
        /// Logs a new message.
        /// </summary>
        /// <param name="message">Message to log.</param>
        public void Log(string message)
        {
            LogWithBuilder(new StringBuilder(message));
        }

        /// <summary>
        /// Logs a new message with an appropriate log header.
        /// </summary>
        private void LogWithBuilder(StringBuilder message)
        {
            Debug.Log(message.Append(LogWriter.StartLogMetadata(_logContext)));
        }
    }
}