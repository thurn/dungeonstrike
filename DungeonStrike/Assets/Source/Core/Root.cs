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
        /// <summary>
        /// Set to 'true' during Editor unit tests to disable some services.
        /// </summary>
        public bool IsUnitTest { get; set; }

        /// <summary>
        /// 'true' if the OnEnable method of this component has started to run.
        /// </summary>
        public bool IsInitialized { get; private set; }

        public void Awake()
        {
            LogWriter.Initialize();
            Debug.Log("Root::Awake()");
        }

        public void OnEnable()
        {
            if (IsInitialized) return;
            // LogWriter static state is lost during serialize/deserialize
            LogWriter.Initialize();
            Debug.Log("Root::OnEnable()");
            IsInitialized = true;

            RegisterServices();
        }

        public void OnDisable()
        {
            Debug.Log("Root::OnDisable()");
            IsInitialized = false;
        }

        /// <summary>
        /// Central registration point for services. Add all service components here. Services should be added in
        /// in dependency order and should not have circular dependencies.
        /// </summary>
        private void RegisterServices()
        {
            AddServiceIfNotPresent<MessageRouter>();
            AddServiceIfNotPresent<SceneLoader>();
            AddServiceIfNotPresent<WebsocketManager>(true);
        }

        /// <summary>
        /// Adds a service to the root object.
        /// </summary>
        /// <param name="omitInTests">If true, avoid adding the service in Editor Unit Tests.</param>
        /// <typeparam name="T">Service Type</typeparam>
        private void AddServiceIfNotPresent<T>(bool omitInTests = false) where T : Service
        {
            if (IsUnitTest && omitInTests) return;
            var components = gameObject.GetComponents<T>();
            if (components.Length == 0)
            {
                gameObject.AddComponent<T>();
            }
        }
    }
}