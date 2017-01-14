using System;
using System.Text;

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
        private readonly DungeonStrikeComponent _component;

        /// <summary>
        /// Constructs a new ErrorHandler.
        /// </summary>
        /// <param name="component">The component which owns this ErrorHandler, included in error reports.</param>
        public ErrorHandler(DungeonStrikeComponent component)
        {
            _component = component;
        }

        /// <see cref="ReportError{T}" />
        public void ReportError(string message)
        {
            ReportError<object>(message);
        }

        /// <summary>
        /// Unconditionally reports that an error has occurred.
        /// </summary>
        /// <param name="message">Error message to report</param>
        /// <param name="value">An anonymous type value containing any relevant local state to log. Any properties of
        /// this object will be logged alongside the error message.</param>
        /// <typeparam name="T">The anonymous type mentioned above.</typeparam>
        /// <exception cref="InvalidOperationException">The exception thrown by this method.</exception>
        public void ReportError<T>(string message, T value = default(T))
        {
            var error = new StringBuilder(message);
            Logger.AppendValueParameters(_component, error, value);
            throw new InvalidOperationException(error.ToString());
        }

        /// <see cref="CheckArgument{T}" />
        public void CheckArgument(bool expression, string message)
        {
            CheckArgument<object>(expression, message);
        }

        /// <summary>
        /// Checks the value of an <paramref name="expression" /> related to method arguments.
        /// </summary>
        /// <param name="expression">The expression to validate.</param>
        /// <param name="message">Error message to report</param>
        /// <param name="value">An anonymous type value containing any relevant local state to log. Any properties of
        /// this object will be logged alongside the error message.</param>
        /// <typeparam name="T">The anonymous type mentioned above.</typeparam>
        /// <exception cref="ArgumentException">The exception thrown by this method if
        /// <paramref name="expression" /> is false.</exception>
        public void CheckArgument<T>(bool expression, string message, T value = default(T))
        {
            if (expression) return;
            var error = new StringBuilder(message);
            Logger.AppendValueParameters(_component, error, value);
            throw new ArgumentException(error.ToString());
        }

        /// <see cref="CheckState{T}" />
        public void CheckState(bool expression, string message)
        {
            CheckState<object>(expression, message);
        }

        /// <summary>
        /// Checks the value of an <paramref name="expression" /> related to program state.
        /// </summary>
        /// <param name="expression">The expression to validate.</param>
        /// <param name="message">Error message to report</param>
        /// <param name="value">An anonymous type value containing any relevant local state to log. Any properties of
        /// this object will be logged alongside the error message.</param>
        /// <typeparam name="T">The anonymous type mentioned above.</typeparam>
        /// <exception cref="ArgumentException">The exception thrown by this method if
        /// <paramref name="expression" /> is false.</exception>
        public void CheckState<T>(bool expression, string message, T value = default(T))
        {
            if (expression) return;
            var error = new StringBuilder(message);
            Logger.AppendValueParameters(_component, error, value);
            throw new InvalidOperationException(error.ToString());
        }

        /// <summary>
        /// Validates that none of the arguments to a method are null.
        /// </summary>
        /// <param name="values">An anonymous type value containing the arguments to check as properties.</param>
        /// <typeparam name="T">The anonymous type mentioned above.</typeparam>
        /// <exception cref="ArgumentNullException">If any of the arguments are null.</exception>
        public void CheckArgumentsNotNull<T>(T values)
        {
            var properties = values.GetType().GetProperties();
            foreach (var property in properties)
            {
                if (property.GetValue(values, null) == null)
                {
                    throw new ArgumentNullException("Argument " + property.Name + " cannot be null");
                }
            }
        }

        /// <summary>
        /// Used to indicate an enum value in a switch statement was not handled.
        /// </summary>
        /// <param name="value">The unhandled enum value.</param>
        /// <returns>An <see cref="Exception"/> which should be thrown.</returns>
        public Exception UnexpectedEnumValue(Enum value)
        {
            return new ArgumentException("Unexpected " + value.GetType() + " enum value: " + value);
        }
    }
}