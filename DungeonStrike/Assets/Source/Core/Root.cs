using System;
using System.Collections.Generic;
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
        private enum State
        {
            NotStarted,
            Starting,
            AsyncStarting,
            Ready,
        }

        /// <summary>
        /// Set to 'true' during Editor unit tests to disable some services.
        /// </summary>
        public bool IsUnitTest { get; set; }

        private List<Action> _onReady = new List<Action>();

        /// <summary>
        /// The number of services which have been started, but which have not yet finished starting.
        /// </summary>
        private int _numPendingServices;

        /// <summary>
        /// The current startup state of services.
        /// </summary>
        private State _state = State.NotStarted;

        private LogContext _rootLogContext;

        private Logger _logger;

        public void Awake()
        {
            LogWriter.Initialize();
        }

        public void OnEnable()
        {
            lock (this)
            {
                SetState(State.Starting);
            }

            // LogWriter static state is lost during serialize/deserialize
            LogWriter.Initialize();
            _rootLogContext = LogContext.NewRootContext(GetType());
            _logger = new Logger(_rootLogContext);
            _logger.Log("Root::OnEnable()");

            RegisterServices();
            lock (this)
            {
                SetState(_numPendingServices == 0 ? State.Ready : State.AsyncStarting);
            }
        }

        public void OnDisable()
        {
            _logger.Log("Root::OnDisable()");
            lock (this)
            {
                SetState(State.NotStarted);
            }
        }

        /// <summary>
        /// Runs an action once all services have completed startup, or immediately if startup has already completed.
        /// Actions added via this method are run in an unspecified order.
        /// </summary>
        /// <param name="action">The action to run after service startup.</param>
        public void RunWhenReady(Action action)
        {
            lock (this)
            {
                if (_state == State.Ready)
                {
                    action();
                }
                else
                {
                    _onReady.Add(action);
                }
            }
        }

        /// <summary>
        /// Called by each service when it begins its startup process.
        /// </summary>
        public void BeganStartingService()
        {
            lock (this)
            {
                _numPendingServices++;
            }
        }

        /// <summary>
        /// Called by each service when it finishes its startup process.
        /// </summary>
        public void FinishedStartingService()
        {
            lock (this)
            {
                _numPendingServices--;
                if ((_state == State.AsyncStarting) && (_numPendingServices == 0))
                {
                    SetState(State.Ready);
                }
            }
        }

        private void SetState(State state)
        {
            if (state == _state)
            {
                throw new ArgumentException("Already in state " + state);
            }
            _state = state;
            if (_state == State.Ready)
            {
                foreach (var action in _onReady)
                {
                    action();
                }
                _onReady = null;
            }
        }

        /// <summary>
        /// Central registration point for services. Add all service components here. Services should be added in
        /// in dependency order and should not have circular dependencies.
        /// </summary>
        private void RegisterServices()
        {
            AddAndEnableService<MessageRouter>();
            AddAndEnableService<SceneLoader>();
            AddAndEnableService<WebsocketManager>(true);
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
                    component.Enable(_rootLogContext);
                    break;
                case 1:
                    components[0].Enable(_rootLogContext);
                    break;
                default:
                    throw new InvalidOperationException("Multiple instances of service found! " + typeof(T));
            }
        }
    }
}