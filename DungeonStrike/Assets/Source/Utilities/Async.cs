using System.Threading.Tasks;

namespace DungeonStrike.Source.Utilities
{
    /// <summary>
    /// Helpers for async programming.
    /// </summary>
    public class Async
    {
        /// <summary>
        /// Placeholder task object to be returned when method signatures require a Task to be returned, but no
        /// async operation is required.
        /// </summary>
        public static readonly Task Done = Task.FromResult(true);
    }
}