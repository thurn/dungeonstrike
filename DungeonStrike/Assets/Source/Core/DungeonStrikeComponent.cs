using System;
using System.Collections.Generic;
using System.Text;
using DungeonStrike.Source.Messaging;
using DungeonStrike.Source.Utilities;
using UnityEngine;

namespace DungeonStrike.Source.Core
{
    /// <summary>
    /// The abstract base class for all <c>MonoBehaviour</c>s in the DungeonStrike project.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>DungeonStrikeBehavior</c> provides useful shared functionality applicable to all or almost all components
    /// in the game, such as logging, error handling, and message routing.
    /// </para>
    ///
    /// <example>
    /// This class sets up the standard message-routing system used by all components. Message subscriptions can be
    /// created by extended either <see cref="Service"/> or <see cref="EntityComponent"/> as follows:
    /// <code>
    /// public sealed class MyComponent : Service
    /// {
    ///     protected override IList&lt;string&gt; SupportedMessageTypes
    ///     {
    ///         get { return new List&lt;string&gt; {"MyMessage"}; }
    ///     }
    ///
    ///     protected override void HandleMessage(Message receivedMessage, Action onComplete)
    ///     {
    ///         var message = (MyMessage) receivedMessage;
    ///         // Handle Message
    ///     }
    /// }
    /// </code>
    /// </example>
    /// </remarks>
    public abstract class DungeonStrikeComponent : MonoBehaviour
    {
        protected enum ComponentLifecycleState
        {
            NotStarted,
            Starting,
            Started,
        }

        /// <summary>
        /// Keeps track of this component's Unity lifecycle state.
        /// </summary>
        protected ComponentLifecycleState LifecycleState = ComponentLifecycleState.NotStarted;

        /// <summary>
        /// Private ILogContext implementation to store information about components.
        /// </summary>
        private class LogContext : ILogContext
        {
            private readonly string _sourceName;
            private readonly string _objectName;

            public LogContext(string sourceName, string objectName)
            {
                _sourceName = sourceName;
                _objectName = objectName;
            }

            public void AppendContextParameters(StringBuilder stringBuilder)
            {
                stringBuilder.Append(":source \"").Append(_sourceName).Append("\", ");
                stringBuilder.Append(":object-name \"").Append(_objectName).Append("\", ");
            }
        }

        private Logger _logger;

        /// <summary>
        /// A <see cref="Logger" /> instance initialized for this component.
        /// </summary>
        protected Logger Logger
        {
            get { return _logger ?? (_logger = new Logger(new LogContext(GetType().ToString(), gameObject.name))); }
        }

        private ErrorHandler _errorHandler;

        /// <summary>
        /// An <see cref="ErrorHandler" /> instance for this component.
        /// </summary>
        protected ErrorHandler ErrorHandler
        {
            get
            {
                return _errorHandler ?? (_errorHandler = new ErrorHandler(
                               new LogContext(GetType().ToString(), gameObject.name)));
            }
        }

        // Root component, used for service lookups.
        private Root _root;

        /// <summary>
        /// Sets the Root object to use for <see cref="GetService&lt;T&gt;()" /> calls.
        /// <para>
        /// This property is *only* intended to be used for unit testing, and must only be set before any service
        /// lookups occur. The <c>DungeonStrikeTest</c> class should be used in lieu of accessing this property
        /// directly, as it offers a <c>CreateTestBehavior()</c> method to populate the root object automatically.
        /// </para>
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the root object is set after a service lookup has
        /// already happened.</exception>
        public Root RootObjectForTests
        {
            get { return _root; }
            set
            {
                if (_root != null)
                {
                    throw new InvalidOperationException("Cannot replace existing root object.");
                }
                _root = value;
            }
        }

        /// <summary>
        /// Returns a specific component on the Root object and ensures that it has been initialized. Must be called
        /// from the main thread.
        /// </summary>
        /// <para>
        /// This implements a singleton pattern for components. Each scene must contain exactly one GameObject with the
        /// <see cref="Root" /> component added to it. Components which only have one logic instance per scene are
        /// called Services, and should be added to the root game object in <see cref="Root.RegisterServices()"/>.
        /// </para>
        /// <typeparam name="T">The type of the component to return.</typeparam>
        /// <returns>The service of type T on the root object.</returns>
        /// <exception cref="InvalidOperationException">Thrown if there is not exactly one GameObject with the Root
        /// component attached to it, or if this service cannot be found.</exception>
        protected T GetService<T>() where T : Service
        {
            ErrorHandler.CheckState(_root.IsInitialized, "Root is not initialized!", typeof(T));
            var results = _root.GetComponents<T>();
            ErrorHandler.CheckState(results.Length != 0, "Unable to locate service", typeof(T));
            ErrorHandler.CheckState(results.Length == 1, "Multiple services found", typeof(T));
            var result = results[0];
            ErrorHandler.CheckState(result.LifecycleState != ComponentLifecycleState.Starting,
                "Circular dependency detected in Service graph", typeof(T));
            if (result.LifecycleState == ComponentLifecycleState.NotStarted)
            {
                // Ensure initialization order is always correct.
                result.OnEnable();
            }
            return result;
        }

        /// <summary>
        /// The standard Unity <c>Awake</c> method, declared here to prevent overriding. Put setup logic in an
        /// appropriate "OnEnable" method instead.
        /// </summary>
        protected void Awake()
        {
        }

        /// <summary>
        /// The standard Unity <c>Start</c> method, declared here to prevent overriding. Put setup logic in an
        /// appropriate "OnEnable" method instead.
        /// </summary>
        protected void Start()
        {
        }

        /// <summary>
        /// The standard Unity <c>OnEnable</c> method, added here for use in test code. If overriding, please ensure
        /// that base.OnEnable() is included as the first line of your implementation.
        /// </summary>
        protected virtual void OnEnable()
        {
            if (_root == null)
            {
                var roots = FindObjectsOfType<Root>();
                ErrorHandler.CheckState(roots.Length == 1, "Exactly one Root object must be created!");
                _root = roots[0];
            }

            if (!_root.IsInitialized)
            {
                // As of 5.5, Unity seems to not properly respect script execution order during hot reloads.
                // Ensure that the Root component has been properly initialized before continuing.
                _root.OnEnable();
            }
        }

        /// <summary>
        /// Exposes <see cref="OnEnable"/> for use in tests.
        /// </summary>
        public void OnEnableForTests()
        {
            OnEnable();
        }

        /// <summary>
        /// The stadard Unity <c>OnDisable</c> method, added here for use in test code.
        /// </summary>
        protected virtual void OnDisable()
        {
        }

        /// <summary>
        /// Exposes <see cref="OnDisable"/> for use in tests.
        /// </summary>
        public void OnDisableForTests()
        {
            OnDisable();
        }

        /// <summary>
        /// Called when this component receives a message from the driver of a type it has registered for.
        /// </summary>
        /// <para>
        /// Overriding this method is the primary way in which components can receive messages from the driver, the
        /// independent program which implements the DungeonStrike game logic. The driver controls the behavior of the
        /// Unity3D frontend by sending JSON-encoded messages to it, which are deserialized to instances of the
        /// <see cref="Message"/> class. Each message must be consumed by exactly one component instance. The component
        /// instance to receive a given message is determined by examining the <see cref="Message.MessageType"/>
        /// property. A component indicates the types of messages it wishes to register for by overriding the
        /// <see cref="SupportedMessageTypes"/> property of this class. When a message is received matching one of your
        /// component's supported message types, this method will be invoked.
        /// </para>
        /// <para>
        /// Message dispatch behavior is affected by the choice of parent class, either <see cref="EntityComponent"/>
        /// or <see cref="Service"/>. Refer to the documentation for those classes to understand the difference.
        /// </para>
        /// <param name="receivedMessage">The new message object received from the driver</param>
        /// <param name="onComplete">A callback which must be invoked once your component has finished processing
        /// <paramref name="receivedMessage"/>. It is an error for a component to be sent a message while it is still
        /// handling a previous message.</param>
        protected virtual void HandleMessage(Message receivedMessage, Action onComplete)
        {
            onComplete();
        }

        /// <summary>
        /// A list of the unique MessageTypes this component wishes to handle.
        /// </summary>
        /// <para>
        /// As discussed in the documentation for <see cref="HandleMessage"/>, this property allows components to
        /// register to receive messages. For singleton-scoped messages, only one instance of your component can
        /// register for a given message type, while for entity-scoped messages, multiple components can register.
        /// </para>
        protected virtual IList<string> SupportedMessageTypes
        {
            get { return new List<string>(); }
        }

        /// <summary>
        /// The ID of the message this component is currently handling, if any.
        /// </summary>
        public Utilities.Optional<string> CurrentMessageId { get; private set; }

        /// <summary>
        /// Top-level method called when a new message is received to perform common bookkeeping functionality.
        /// </summary>
        /// <para>
        /// This method is invoked by <see cref="MessageRouter"/> when messages are received. It should not be invoked
        /// from component code.
        /// </para>
        /// <param name="message">The received message object.</param>
        public void HandleMessageFromDriver(Message message)
        {
            Logger.Log("Received message", message.MessageId);
            ErrorHandler.CheckState(!CurrentMessageId.HasValue, "Component is already handling a message");
            CurrentMessageId = Optional.Of(message.MessageId);
            HandleMessage(message, () =>
            {
                Logger.Log("Finished processing message", message.MessageId);
                CurrentMessageId = Utilities.Optional<string>.Empty;
            });
        }
    }
}