namespace DungeonStrike
{
    public class Preconditions
    {
        public static void CheckArgument(bool expression, string message = "")
        {
            if (!expression)
            {
                throw new System.ArgumentException(message);
            }
        }

        public static void CheckState(bool expression, string message = "")
        {
            if (!expression)
            {
                throw new System.InvalidOperationException(message);
            }
        }

        public static void CheckNotNull(object valueToCheck, string message = "")
        {
            if (valueToCheck == null)
            {
                throw new System.ArgumentNullException(message);
            }
        }
    }
}