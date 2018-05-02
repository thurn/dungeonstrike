using System.Collections.Generic;
using System.Threading.Tasks;
using DungeonStrike.Source.Core;
using DungeonStrike.Source.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DungeonStrike.Source.Tools
{
    /// <summary>
    /// Panopticon is a service which is intended to operate only during development and running tests. It monitors
    /// the state of the entities present in the game and emits log output when their state changes in significant
    /// ways.
    /// </summary>
    public sealed class Panopticon : Service
    {
        private readonly ISet<GameObject> _trackedEntities = new HashSet<GameObject>();

        protected override Task<Result> OnEnableService()
        {
            SceneManager.sceneLoaded += (scene, mode) => Logger.Log("Scene loaded", scene.name);
            return Async.Success;
        }

        private void Update()
        {
            foreach (var gameObject in UnityEngine.SceneManagement
                    .SceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (_trackedEntities.Contains(gameObject)) continue;
                Logger.Log("Object created", gameObject.name);
                _trackedEntities.Add(gameObject);
            }
        }
    }
}
