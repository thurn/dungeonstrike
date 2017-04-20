using System;
using System.Collections.Generic;
using DisruptorUnity3d;
using DungeonStrike.Source.Messaging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Tuple = DungeonStrike.Source.Utilities.Tuple;

namespace DungeonStrike.Source.Core
{
    /// <summary>
    /// Service which handles message dispatch to components
    /// </summary>
    public sealed class MessageRouter : Service
    {
        // True if 'OnEnable' has been been invoked since the last 'OnDisable'.
        private bool _initialized;

        // Map from message type to the registered message handler
        private Dictionary<string, DungeonStrikeComponent> _serviceMessageHandlers;

        // Map from (message type, entityId) to the registered message handler
        private Dictionary<Utilities.Tuple<string, string>, DungeonStrikeComponent> _entityComponentMessageHandlers;

        private RingBuffer<Exception> _errors;

        private RingBuffer<Utilities.Tuple<Message, DungeonStrikeComponent>> _messages;

        // Captured ErrorHandler instance, since ErrorHandler property can only be accessed from the main thread.
        private ErrorHandler _errorHandler;

        /// <summary>
        /// Initialize the message router. Called from the OnEnable method of each entity and service component.
        /// </summary>
        /// <para>
        /// We avoid using the normal 'OnEnable' method because this component is used by every other component's
        /// OnEnable method.
        /// </para>
        public void Initialize()
        {
            if (_initialized) return;
            _serviceMessageHandlers = new Dictionary<string, DungeonStrikeComponent>();
            _entityComponentMessageHandlers =
                new Dictionary<Utilities.Tuple<string, string>, DungeonStrikeComponent>();
            _errors = new RingBuffer<Exception>(16);
            _messages = new RingBuffer<Utilities.Tuple<Message, DungeonStrikeComponent>>(16);
            _errorHandler = ErrorHandler;
            _initialized = true;
        }

        protected override void OnDisable()
        {
            _initialized = false;
            _serviceMessageHandlers = null;
            _entityComponentMessageHandlers = null;
            _errors = null;
            _messages = null;
        }

        public void Update()
        {
            if (!_initialized) return;

            Exception error;
            if (_errors.TryDequeue(out error))
            {
                throw error;
            }

            Utilities.Tuple<Message, DungeonStrikeComponent> messageTarget;
            if (_messages.TryDequeue(out messageTarget))
            {
                messageTarget.Item2.HandleMessageFromDriver(messageTarget.Item1);
            }
        }

        /// <summary>
        /// Register a <see cref="Service"/> to be the exclusive receiver of a specific type of messages
        /// </summary>
        /// <param name="messageType">The specific type of messages to register for</param>
        /// <param name="service">The service object to call when such a message is received</param>
        public void RegisterServiceForMessage(string messageType, Service service)
        {
            ErrorHandler.CheckNotNull("messageType", messageType, "service", service);
            ErrorHandler.CheckArgument(!_serviceMessageHandlers.ContainsKey(messageType),
                "Handler already registered for message", "messageType");
            _serviceMessageHandlers[messageType] = service;
        }

        /// <summary>
        /// Register an <see cref="EntityComponent"/> to be the receiver of a specific type of messages
        /// </summary>
        /// <param name="messageType">The type of messages to register for</param>
        /// <param name="entityId">The entityId of messages that should be delivered, based on
        /// <see cref="Message.EntityId"/></param>
        /// <param name="component">The component which should be called when these messages are received</param>
        public void RegisterEntityComponentForMessage(string messageType, string entityId, EntityComponent component)
        {
            ErrorHandler.CheckNotNull("messageType", messageType, "entityId", entityId, "component", component);
            var key = Tuple.Create(messageType, entityId);
            ErrorHandler.CheckArgument(!_entityComponentMessageHandlers.ContainsKey(key),
                "Handler already registered for message", "messageType", "entityId", entityId);
            _entityComponentMessageHandlers[key] = component;
        }

        /// <summary>
        /// Called when new messages are received from the driver. Should not be invoked in user code.
        /// </summary>
        /// <param name="messageInput">The newly-received message JSON string, which should be delivered to the
        /// frontend.</param>
        public void RouteMessageToFrontend(string messageInput)
        {
            try
            {
                var message = JsonConvert.DeserializeObject<Message>(messageInput, new MessageConverter(),
                        new StringEnumConverter());
                _errorHandler.CheckNotNull("message", message);
                var messageType = message.MessageType;
                if (message.EntityId != null)
                {
                    var key = Tuple.Create(messageType, message.EntityId);
                    _errorHandler.CheckState(_entityComponentMessageHandlers.ContainsKey(key),
                        "No entity component message handler registered for message", "MessageId", message.MessageId,
                        "MessageType", message.MessageType, "EntityId", message.EntityId);
                    _messages.Enqueue(Tuple.Create(message, _entityComponentMessageHandlers[key]));
                }
                else
                {
                    _errorHandler.CheckArgument(_serviceMessageHandlers.ContainsKey(messageType),
                        "No service message handler registered for message", "MessageId", message.MessageId,
                        "MessageType", message.MessageType);
                    _messages.Enqueue(Tuple.Create(message, _serviceMessageHandlers[messageType]));
                }
            }
            catch (Exception ex)
            {
                // Forward exceptions to the UI thread so WebSocketSharp doesn't swallow them.
                _errors.Enqueue(ex);
                throw;
            }
        }
    }
}