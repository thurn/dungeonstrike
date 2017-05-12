using System;
using System.Collections.Generic;

namespace DungeonStrike.Source.Utilities
{
    /// <summary>
    /// A class that simplifies invoking a completion callback after the last of several parallel tasks have finished
    /// executing.
    /// </summary>
    public class Completer
    {
        private readonly Dictionary<string, bool> _completed = new Dictionary<string, bool>();
        private readonly Action _onComplete;
        private int _numPending;

        /// <summary>
        /// Creates a new Completer with an action to invoke once all of the provided keys are completed.
        /// </summary>
        /// <param name="onComplete">Action to invoke after the <see cref="Complete(string)"/> method has been invoked
        /// for each key.</param>
        /// <param name="completionKeys">Array of keys to expect.</param>
        public Completer(Action onComplete, params string[] completionKeys)
        {
            _numPending = completionKeys.Length;
            _onComplete = onComplete;

            foreach (var key in completionKeys)
            {
                _completed[key] = false;
            }
        }

        /// <summary>
        /// Called to indicate that the task with the task key 'key' has completed. Calling this method multiple times
        /// with the same key has no effect.
        /// </summary>
        /// <param name="key">Completion key for the task.</param>
        public void Complete(string key)
        {
            lock (this)
            {
                if (!_completed.ContainsKey(key))
                {
                    throw new ArgumentException("Unrecognized completion key: '" + key + "'");
                }
                if (!_completed[key])
                {
                    _completed[key] = true;
                    _numPending--;
                    if (_numPending == 0)
                    {
                        _onComplete();
                    }
                }
            }
        }
    }
}