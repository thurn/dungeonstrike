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
        /// <param name="logContext">The log context in which to log.</param>
        public Logger(LogContext logContext)
        {
            _logContext = logContext;
        }

        /// <summary>
        /// Logs a new message.
        /// </summary>
        /// <param name="message">Message to log.</param>
        /// <param name="arguments">Additional log parameters</param>
        public void Log(string message, params object[] arguments)
        {
            // AllowBannedRegex:
            Debug.Log(LogWriter.FormatForLogOutput(message, _logContext, false, arguments));
        }

        /// <returns>The client ID associated with the current log context.</returns>
        public string CurrentClientId()
        {
            return _logContext.ClientId;
        }
    }
}