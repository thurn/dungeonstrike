using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        /// The private root object property.
        /// </summary>
        private Root _root;

        /// <summary>
        /// Keeps track of this component's Unity lifecycle state.
        /// </summary>
        protected ComponentLifecycleState LifecycleState = ComponentLifecycleState.NotStarted;

        /// <summary>
        /// The private log context of this component.
        /// </summary>
        private LogContext _logContext;

        private Logger _logger;

        /// <summary>
        /// A <see cref="Logger" /> instance initialized for this component.
        /// </summary>
        protected Logger Logger
        {
            get
            {
                if (_logger == null)
                {
                    throw new InvalidOperationException("Attempted to access logger before Enable().");
                }
                return _logger;
            }
        }

        private ErrorHandler _errorHandler;

        /// <summary>
        /// An <see cref="ErrorHandler" /> instance for this component.
        /// </summary>
        protected ErrorHandler ErrorHandler
        {
            get
            {
                if (_errorHandler == null)
                {
                    throw new InvalidOperationException("Attempted to access error handler before Enable().");
                }
                return _errorHandler;
            }
        }

        /// <summary>
        /// Method which should be called by the component creator to activate each component.
        /// </summary>
        /// <para>
        /// DungeonStrikeComponents do not participate in the normal Unity Awake->OnEnable->Start lifecycle. Their
        /// initial 'startup' logic is instead invoked by a call to this method, typically immediately after a component
        /// is created. The <paramref name="parentContext"/> parameter is used to track the chain of ownership between
        /// a component and its creator.
        /// </para>
        /// <para>
        /// If this method is overridden, the first line of the new implementation should invoke
        /// "base.Enable(parentContext)".
        /// </para>
        /// <param name="parentContext">The LogContext of the component which created this component.</param>
        protected void Initialize(LogContext parentContext)
        {
            if (_root == null)
            {
                throw new InvalidOperationException("Root must be specified before calling Enable()!");
            }
            var logContext = LogContext.NewContext(parentContext, GetType(), gameObject);
            _logContext = logContext;
            _logger = new Logger(logContext);
            _errorHandler = new ErrorHandler(logContext);
        }

        /// <summary>
        /// Method to perform initialization logic for components. Must be called immediately after creating
        /// a new DungeonStrikeComponent. Implementations should call <see cref="Initialize"/> as their first action.
        /// </summary>
        /// <param name="parentContext">The <see cref="LogContext"/> of the component which created this component.
        /// Used to form a contextual chain tracing how components are instantiated.</param>
        /// <returns></returns>
        public abstract Task<DungeonStrikeComponent> Enable(LogContext parentContext);

        /// <summary>
        /// A reference to the global root component, used for service lookups. This property should be set by the
        /// creator of a component, immediately after creation. It is an error to modify this property after it has
        /// been set.
        /// </summary>
        public Root Root
        {
            get { return _root; }
            set
            {
                if (_root != null)
                {
                    throw new InvalidOperationException("Root property cannot be changed!");
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
        /// called Services, and should be added to the root game object in <see cref="Root#RegisterServices()"/>.
        /// </para>
        /// <typeparam name="T">The type of the component to return.</typeparam>
        /// <returns>The service of type T on the root object.</returns>
        /// <exception cref="InvalidOperationException">Thrown if there is not exactly one GameObject with the Root
        /// component attached to it, or if this service cannot be found.</exception>
        protected Task<T> GetService<T>() where T : Service
        {
            if (_root == null)
            {
                throw new InvalidOperationException("Root must be specified before calling GetService()!");
            }
            return _root.GetService<T>();
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
        /// appropriate method like <c>OnEnableService()</c> instead.
        /// </summary>
        protected void Start()
        {
        }

        /// <summary>
        /// The standard Unity <c>OnEnable</c> method, declared here to prevent overriding. Put setup logic in an
        /// appropriate method like <c>OnEnableService()</c> instead.
        /// </summary>
        protected void OnEnable()
        {
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
        /// The unique MessageType for this component, or null if this component is not associated with any message
        /// type.
        /// </summary>
        /// <para>
        /// As discussed in the documentation for <see cref="HandleMessage"/>, this property allows components to
        /// register to receive messages. For singleton-scoped messages, only one instance of your component can
        /// register for a given message type, while for entity-scoped messages, multiple components can register.
        /// </para>
        protected virtual string MessageType => null;

        /// <summary>
        /// Starts a coroutine to run a Unity YieldInstruction-based operation asynchronously wrapped in a Task.
        /// </summary>
        protected Task RunOperationAsync(YieldInstruction operation)
        {
            var completionSource = new TaskCompletionSource<bool>();
            StartCoroutine(RunOperationAsyncEnumerator(operation, completionSource));
            return completionSource.Task;
        }

        private static IEnumerator<YieldInstruction> RunOperationAsyncEnumerator(YieldInstruction operation,
            TaskCompletionSource<bool> completionSource)
        {
            yield return operation;
            completionSource.SetResult(true);
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
        public async Task HandleMessageFromDriver(Message message)
        {
            Logger.Log("Received message", message);
            ErrorHandler.CheckState(!CurrentMessageId.HasValue, "Component is already handling a message");
            CurrentMessageId = Optional.Of(message.MessageId);
            var result = await HandleMessage(message);

            Logger.Log(result == Result.Success ? "Finished processing message" : "Error processing message", message);
            CurrentMessageId = Utilities.Optional<string>.Empty;
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
        /// <see cref="MessageType"/> property of this class. When a message is received matching one of your
        /// component's supported message types, this method will be invoked.
        /// </para>
        /// <para>
        /// Message dispatch behavior is affected by the choice of parent class, either <see cref="EntityComponent"/>
        /// or <see cref="Service"/>. Refer to the documentation for those classes to understand the difference.
        /// </para>
        /// <param name="receivedMessage">The new message object received from the driver</param>
        /// <returns>A Task object which should be completed once the message has been handled.</returns>
        protected virtual Task<Result> HandleMessage(Message receivedMessage)
        {
            return Async.Success;
        }

        /// <summary>
        /// Adds a new DungeonStrikeComponent to a given GameObject and immediately enables it. The new componet
        /// is initialized as a child of this component, meaning that it will have a derived LogContext and share the
        /// same root object reference.
        /// </summary>
        /// <param name="source">The game object to which the component should be added.</param>
        /// <typeparam name="T">The type of the component to add.</typeparam>
        /// <returns></returns>
        public async Task<T> AddAndEnableComponent<T>(GameObject source) where T : DungeonStrikeComponent
        {
            var result = source.AddComponent<T>();
            result.Root = _root;
            await result.Enable(_logContext);

            return result;
        }
    }
}