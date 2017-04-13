using System.Text;

namespace DungeonStrike.Source.Core
{
    /// <summary>
    /// Represents call-site contextual information about a log entry or error message. Should not be implemented
    /// except by the private implementation in DungeonStrikeComponent.
    /// </summary>
    public interface ILogContext
    {
        /// <summary>
        /// Appends the contextual information about this LogContext to the provided stringBuilder.
        /// </summary>
        /// <param name="stringBuilder">Input stringBuilder</param>
        void AppendContextParameters(StringBuilder stringBuilder);
    }
}