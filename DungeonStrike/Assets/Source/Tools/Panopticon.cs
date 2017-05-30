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

        protected override Task OnEnableService()
        {
            SceneManager.sceneLoaded += (scene, mode) => Logger.Log("Scene loaded", scene.name);
            return Async.Done;
        }

        private void Update()
        {
            var newEntities = FindObjectsOfType<Entity>();
            foreach (var entity in newEntities)
            {
                if (_trackedEntities.Contains(entity.gameObject)) continue;
                Logger.Log("Entity created", entity);
                _trackedEntities.Add(entity.gameObject);
            }
        }
    }
}
