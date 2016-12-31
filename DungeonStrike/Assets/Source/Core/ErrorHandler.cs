using System;
using System.Text;

namespace DungeonStrike.Assets.Source.Core
{
    public sealed class ErrorHandler
    {
        private DungeonStrikeBehavior _behaviour;

        public ErrorHandler(DungeonStrikeBehavior behaviour)
        {
            _behaviour = behaviour;
        }

        public void ReportError<T>(string message, Func<T> value = null)
        {
            var error = new StringBuilder(message);
            AppendValueParameters(error, value);
            throw new InvalidOperationException(error.ToString());
        }

        public void CheckArgument<T>(bool expression, string message, Func<T> value = null)
        {
            if (expression) return;
            var error = new StringBuilder(message);
            AppendValueParameters(error, value);
            throw new ArgumentException(error.ToString());
        }

        public void CheckState(bool expression, string message)
        {
            CheckState<object>(expression, message, null);
        }

        public void CheckState<T>(bool expression, string message, Func<T> value = null)
        {
            if (expression) return;
            var error = new StringBuilder(message);
            AppendValueParameters(error, value);
            throw new InvalidOperationException(error.ToString());
        }

        public void CheckNotNull<T>(T values)
        {
            var properties = values.GetType().GetProperties();
            foreach (var property in properties)
            {
                if (property.GetValue(values, null) == null)
                {
                    throw new ArgumentException("Value " + property.Name + " cannot be null");
                }
            }
        }

        private static void AppendValueParameters<T>(StringBuilder builder, Func<T> value = null)
        {
            if (value == null) return;
            var values = value();
            var properties = values.GetType().GetProperties();
            foreach (var property in properties)
            {
                builder.Append("\n").Append(property.Name).Append("=").Append(property.GetValue(values, null));
            }
        }

        public Exception UnexpectedEnumValue(Enum value)
        {
            return new ArgumentException("Unexpected " + value.GetType() + " enum value: " + value);
        }
    }
}