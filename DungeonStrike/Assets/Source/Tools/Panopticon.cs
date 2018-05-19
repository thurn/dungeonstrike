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
        private readonly HashSet<GameObject> _trackedEntities = new HashSet<GameObject>();
        private ComponentTracker _componentTracker;

        protected override Task<Result> OnEnableService()
        {
            SceneManager.sceneLoaded += (scene, mode) => Logger.Log("Scene loaded", scene.name);
            _componentTracker = new ComponentTracker(Logger);
            InvokeRepeating(nameof(ObserveObjects), 2.0f, 2.0f);
            return Async.Success;
        }

        private void ObserveObjects()
        {
            foreach (var go in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                ObserveObject(go);
            }
        }

        private void ObserveObject(GameObject observee)
        {
            if (observee.CompareTag("Environment"))
            {
                return;
            }

            if (!_trackedEntities.Contains(observee))
            {
                Logger.Log("Object created", observee.name);
                _trackedEntities.Add(observee);
            }


            if (!observee.CompareTag("Prefab"))
            {
                foreach (var component in observee.GetComponents<Component>())
                {
                    _componentTracker.TrackComponentState(observee, component);
                }

                foreach (Transform t in observee.transform)
                {
                    ObserveObject(t.gameObject);
                }
            }
        }
    }
}
