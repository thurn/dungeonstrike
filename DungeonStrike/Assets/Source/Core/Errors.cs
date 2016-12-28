using System;

namespace DungeonStrike.Core
{
    public class Errors
    {
        public static void CheckArgument(bool expression, string message = "")
        {
            if (!expression)
            {
                throw new ArgumentException(message);
            }
        }

        public static void CheckState(bool expression, string message = "")
        {
            if (!expression)
            {
                throw new InvalidOperationException(message);
            }
        }

        public static void CheckNotNull(object valueToCheck, string message = "")
        {
            if (valueToCheck == null)
            {
                throw new ArgumentNullException(message);
            }
        }

        public static Exception UnexpectedEnumValue(Enum value)
        {
            return new SystemException("Unexpected " + value.GetType() + " enum value: " + value);
        }
    }
}