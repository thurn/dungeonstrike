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
        private readonly DungeonStrikeComponent _component;

        /// <summary>
        /// Create a new Logger.
        /// </summary>
        /// <param name="component">The component which owns this Logger, included in logs.</param>
        public Logger(DungeonStrikeComponent component)
        {
            _component = component;
        }

        /// <summary>
        /// Logs a new message.
        /// </summary>
        /// <param name="message">Message to log.</param>
        public void Log(string message)
        {
            Debug.Log(message);
        }

        /// <summary>
        /// Logs a new message and some associated values.
        /// </summary>
        /// <param name="message">Message to log.</param>
        /// <param name="value">Anonymous type value containing the associated values to log.</param>
        /// <typeparam name="T">The anyonmous type of <paramref name="value"/></typeparam>
        public void Log<T>(string message, T value)
        {
            var messageBuilder = new StringBuilder(message);
            AppendValueParameters(_component, messageBuilder, value);
            Debug.Log(message);
        }

        /// <summary>
        /// Appends the string representation of the properties of <paramref name="value" /> to
        /// <paramref name="builder" />.
        /// </summary>
        /// <param name="component">The <see cref="DungeonStrikeComponent"/> creating this string.</param>
        /// <param name="builder">The <see cref="StringBuilder"/> to which the properties should be appended.</param>
        /// <param name="value">An anonymous type value whose properties contain the values to append.</param>
        /// <typeparam name="T">The type of <paramref name="value" />.</typeparam>
        public static void AppendValueParameters<T>(DungeonStrikeComponent component, StringBuilder builder,
            T value = default(T))
        {
            if (value == null) return;
            var properties = value.GetType().GetProperties();
            builder.Append("\n").Append("In ").Append(component);
            foreach (var property in properties)
            {
                builder.Append("\n").Append(property.Name).Append("=").Append(property.GetValue(value, null));
            }
        }
    }
}