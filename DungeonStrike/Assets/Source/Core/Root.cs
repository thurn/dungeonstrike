﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using DungeonStrike.Source.Assets;
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
        private enum LifecycleState
        {
            NotStarted,
            Starting,
            Ready,
        }

        /// <summary>
        /// Set to 'true' during Editor unit tests to disable some services.
        /// </summary>
        public bool IsUnitTest { get; set; }

        private readonly ConcurrentQueue<Action> _onReady = new ConcurrentQueue<Action>();

        /// <summary>
        /// The current startup state of services.
        /// </summary>
        private LifecycleState _state;

        private LifecycleState State
        {
            get
            {
                lock (this)
                {
                    return _state;
                }
            }
            set
            {
                lock (this)
                {
                    _state = value;
                }
            }
        }

        private LogContext _rootLogContext;

        private Logger _logger;

        private ErrorHandler _errorHandler;

        private readonly IDictionary<Type, Task<Service>> _services = new Dictionary<Type, Task<Service>>();

        private static Root _instance;

        public void Awake()
        {
            if (IsUnitTest) return;

            if (_instance != null && _instance != this)
            {
                // Each scene has a Root object. This means that when a new scene is loaded, there will be two Roots
                // present. If this happens, we destroy the duplicate.
                DestroyImmediate(gameObject);
            }
            else
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                LogWriter.Initialize();
            }
        }

        public async void OnEnable()
        {
            State = LifecycleState.Starting;
            // LogWriter static state is lost during serialize/deserialize
            LogWriter.Initialize();
            _rootLogContext = LogContext.NewRootContext(GetType());
            _logger = new Logger(_rootLogContext);
            _errorHandler = new ErrorHandler(_rootLogContext);
            _logger.Log("Root::OnEnable()");

            RegisterServices();
            await Task.WhenAll(_services.Values);

            State = LifecycleState.Ready;
            foreach (var action in _onReady)
            {
                action();
            }
        }

        public void OnDisable()
        {
            if (_logger == null) return;
            _logger.Log("Root::OnDisable()");
            State = LifecycleState.NotStarted;
        }

        /// <summary>
        /// Runs an action once all services have completed startup, or immediately if startup has already completed.
        /// Actions added via this method are run in an unspecified order.
        /// </summary>
        /// <param name="action">The action to run after service startup.</param>
        public void RunWhenReady(Action action)
        {
            if (State == LifecycleState.Ready)
            {
                action();
            }
            else
            {
                _onReady.Enqueue(action);
            }
        }

        public async Task<T> GetService<T>() where T : Service
        {
            var task = _services[typeof(T)];
            if (task == null)
            {
                throw _errorHandler.NewException("Service not found", typeof(T));
            }
            return (T) await task;
        }

        /// <summary>
        /// Central registration point for services. Add all service components here. Services should be added in
        /// in dependency order and should not have circular dependencies.
        /// </summary>
        private void RegisterServices()
        {
            AddAndEnableService<MessageRouter>();
            AddAndEnableService<WebsocketManager>(true);
            AddAndEnableService<AssetLoader>();
            AddAndEnableService<SceneLoader>();
            AddAndEnableService<QuitGame>();
            AddAndEnableService<CreateEntity>();
        }

        /// <summary>
        /// Enables a services, adding it to the root object if it is not already present.
        /// </summary>
        /// <param name="omitInTests">If true, avoid adding the service in Editor Unit Tests.</param>
        /// <typeparam name="T">Service Type</typeparam>
        private void AddAndEnableService<T>(bool omitInTests = false) where T : Service
        {
            if (IsUnitTest && omitInTests) return;
            var components = gameObject.GetComponents<T>();
            switch (components.Length)
            {
                case 0:
                    var component = gameObject.AddComponent<T>();
                    component.Root = this;
                    _services[typeof(T)] = component.Enable(_rootLogContext);
                    break;
                case 1:
                    _services[typeof(T)] = components[0].Enable(_rootLogContext);
                    break;
                default:
                    throw new InvalidOperationException("Multiple instances of service found! " + typeof(T));
            }
        }
    }
}