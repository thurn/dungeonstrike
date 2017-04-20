using DungeonStrike.Source.Messaging;
using DungeonStrike.Source.Services;
using UnityEngine;

namespace DungeonStrike.Source.Core
{
    /// <summary>
    /// Central registration system for <see cref="Service"/> components.
    /// </summary>
    /// <remarks>
    /// The Root component should be added to exactly one GameObject in each scene. It is responsible for registering
    /// all <see cref="Service"/> components in its OnEnable() method. These components can retrieved by calling
    /// <see cref="DungeonStrikeComponent.GetService{T}"/>.
    /// </remarks>
    public sealed class Root : MonoBehaviour
    {
        public void Awake()
        {
            LogWriter.Initialize();
        }

        public void OnEnable()
        {
            LogWriter.Initialize();
            Debug.Log("System Enabled ");
            Application.logMessageReceivedThreaded += LogWriter.HandleUnityLog;
            DestroyAllServices();
            RegisterServices(false);
        }

        public void OnDisable()
        {
            Application.logMessageReceivedThreaded -= LogWriter.HandleUnityLog;
        }

        private void DestroyAllServices()
        {
            foreach (var service in GetComponents<Service>())
            {
                // Must DestroyImmediate, or else you can get into very confusing states with multiple copies of the
                // same service.
                DestroyImmediate(service);
            }
        }

        /// <summary>
        /// Central registration point for services. Add all service components here. This method should only be
        /// invoked from test code.
        /// <param name="forTests">If ture, only register services appropriate for a unit test.</param>
        /// </summary>
        public void RegisterServices(bool forTests)
        {
            gameObject.AddComponent<MessageRouter>();
            if (!forTests)
            {
                gameObject.AddComponent<WebsocketManager>();
            }
            gameObject.AddComponent<SceneLoader>();
        }
    }
}