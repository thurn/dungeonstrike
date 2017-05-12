using System;

namespace DungeonStrike.Source.Core
{
    /// <summary>
    /// Provides utility methods for checking for and reporting programmer errors.
    /// </summary>
    /// <remarks>
    /// This class provides several methods which can be employed to check for various types of programming errors,
    /// such as passing invalid argument values or receiving an unexpected enum value in a switch statement. It is
    /// *not* intended to handle user errors -- invalid user actions should be communicated via the game UI or, ideally,
    /// not be allowed to occur in the first place.
    /// </remarks>
    public sealed class ErrorHandler
    {
        private readonly LogContext _logContext;

        /// <summary>
        /// Create a new ErrorHandler.
        /// </summary>
        /// <param name="logContext">The log context in which to log.</param>
        public ErrorHandler(LogContext logContext)
        {
            _logContext = logContext;
        }

        /// <summary>
        /// Unconditionally throws an error.
        /// </summary>
        public void ReportError(string message, params object[] arguments)
        {
            throw new SystemException(LogWriter.FormatForLogOutput(message, _logContext, true, arguments));
        }

        /// <summary>
        /// Throws an ArgumentException if "expression" is false.
        /// </summary>
        public void CheckArgument(bool expression, string message, params object[] arguments)
        {
            if (expression) return;
            throw new ArgumentException(LogWriter.FormatForLogOutput(message, _logContext, true, arguments));
        }

        /// <summary>
        /// Throws an InvalidOperationException if "expression" is false.
        /// </summary>
        public void CheckState(bool expression, string message, params object[] arguments)
        {
            if (expression) return;
            throw new InvalidOperationException(LogWriter.FormatForLogOutput(message, _logContext, true, arguments));
        }

        /// <summary>
        /// Throws an ArgumentNullException if a parameter is null.
        /// </summary>
        public void CheckNotNull(string parameterName, object parameterValue)
        {
            if (parameterValue == null)
            {
                throw new ArgumentNullException(LogWriter.FormatForLogOutput("Parameter cannot be null",
                        _logContext, true, new object[] {parameterName, "null"}));
            }
        }

        /// <summary>
        /// Throws an ArgumentNullException if a parameter is null.
        /// </summary>
        public void CheckNotNull(string param1, object value1, string param2, object value2)
        {
            if (value1 == null)
            {
                throw new ArgumentNullException(LogWriter.FormatForLogOutput("Parameter cannot be null",
                        _logContext, true, new object[] {param1, "null"}));
            }
            if (value2 == null)
            {
                throw new ArgumentNullException(LogWriter.FormatForLogOutput("Parameter cannot be null",
                        _logContext, true, new object[] {param2, "null"}));
            }
        }

        /// <summary>
        /// Throws an ArgumentNullException if a parameter is null.
        /// </summary>
        public void CheckNotNull(string param1, object value1, string param2, object value2,
            string param3, object value3)
        {
            if (value1 == null)
            {
                throw new ArgumentNullException(LogWriter.FormatForLogOutput("Parameter cannot be null",
                        _logContext, true, new object[] {param1, "null"}));
            }
            if (value2 == null)
            {
                throw new ArgumentNullException(LogWriter.FormatForLogOutput("Parameter cannot be null",
                        _logContext, true, new object[] {param2, "null"}));
            }
            if (value3 == null)
            {
                throw new ArgumentNullException(LogWriter.FormatForLogOutput("Parameter cannot be null",
                        _logContext, true, new object[] {param3, "null"}));
            }
        }

        /// <summary>
        /// Throws an ArgumentNullException if a parameter is null.
        /// </summary>
        public void CheckNotNull(string param1, object value1, string param2, object value2,
            string param3, object value3, string param4, object value4)
        {
            if (value1 == null)
            {
                throw new ArgumentNullException(LogWriter.FormatForLogOutput("Parameter cannot be null",
                        _logContext, true, new object[] {param1, "null"}));
            }
            if (value2 == null)
            {
                throw new ArgumentNullException(LogWriter.FormatForLogOutput("Parameter cannot be null",
                        _logContext, true, new object[] {param2, "null"}));
            }
            if (value3 == null)
            {
                throw new ArgumentNullException(LogWriter.FormatForLogOutput("Parameter cannot be null",
                        _logContext, true, new object[] {param3, "null"}));
            }
            if (value4 == null)
            {
                throw new ArgumentNullException(LogWriter.FormatForLogOutput("Parameter cannot be null",
                        _logContext, true, new object[] {param4, "null"}));
            }
        }

        /// <summary>
        /// Throws an ArgumentNullException if a parameter is null.
        /// </summary>
        public void CheckNotNull(string param1, object value1, string param2, object value2,
            string param3, object value3, string param4, object value4, string param5, object value5)
        {
            if (value1 == null)
            {
                throw new ArgumentNullException(LogWriter.FormatForLogOutput("Parameter cannot be null",
                        _logContext, true, new object[] {param1, "null"}));
            }
            if (value2 == null)
            {
                throw new ArgumentNullException(LogWriter.FormatForLogOutput("Parameter cannot be null",
                        _logContext, true, new object[] {param2, "null"}));
            }
            if (value3 == null)
            {
                throw new ArgumentNullException(LogWriter.FormatForLogOutput("Parameter cannot be null",
                        _logContext, true, new object[] {param3, "null"}));
            }
            if (value4 == null)
            {
                throw new ArgumentNullException(LogWriter.FormatForLogOutput("Parameter cannot be null",
                        _logContext, true, new object[] {param4, "null"}));
            }
            if (value5 == null)
            {
                throw new ArgumentNullException(LogWriter.FormatForLogOutput("Parameter cannot be null",
                        _logContext, true, new object[] {param5, "null"}));
            }
        }

        /// <summary>
        /// Used to indicate an enum value in a switch statement was not handled.
        /// </summary>
        /// <param name="value">The unhandled enum value.</param>
        /// <returns>An <see cref="Exception"/> which should be thrown.</returns>
        public Exception UnexpectedEnumValue(Enum value)
        {
            return new ArgumentException(LogWriter.FormatForLogOutput("Unexpected enum value", _logContext, true,
                    new object[] {"type", value.GetType(), "enumValue", value}));
        }
    }
}