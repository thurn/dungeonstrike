using System.Threading.Tasks;

namespace DungeonStrike.Source.Utilities
{
    /// <summary>
    /// Enum indicating the outcome of an asynchronous operation.
    /// </summary>
    public enum Result
    {
        // The operation completed successfully.
        Success,

        // The operation failed to complete.
        Failure
    }

    /// <summary>
    /// Helpers for async programming.
    /// </summary>
    public class Async
    {
        /// <summary>
        /// Task object to be returned to indicate success.
        /// </summary>
        public static readonly Task<Result> Success = Task.FromResult(Result.Success);

        /// <summary>
        /// Task object to be returned to indicate failure.
        /// </summary>
        public static readonly Task<Result> Error = Task.FromResult(Result.Failure);
    }
}