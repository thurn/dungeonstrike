using System;

namespace DungeonStrike.Assets.Source.Core
{
    public sealed class StandardErrorHandler : IErrorHandler
    {
        private DungeonStrikeBehavior _behaviour;

        internal StandardErrorHandler(DungeonStrikeBehavior behaviour)
        {
            _behaviour = behaviour;
        }

        public void ReportError<T>(string message, T value)
        {
        }

        public void CheckArgument<T>(bool expression, string message = "", T value = default(T))
        {
            if (!expression)
            {
                throw new ArgumentException(message);
            }
        }

        public void CheckState(bool expression, string message = "")
        {
            if (!expression)
            {
                throw new InvalidOperationException(message);
            }
        }

        public void CheckState<T>(bool expression, string message, T value)
        {
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

        public Exception UnexpectedEnumValue(Enum value)
        {
            return new ArgumentException("Unexpected " + value.GetType() + " enum value: " + value);
        }
    }
}