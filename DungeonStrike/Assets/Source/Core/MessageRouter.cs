using System.Collections.Generic;
using DisruptorUnity3d;
using DungeonStrike.Source.Messaging;
using DungeonStrike.Source.Utilities;

namespace DungeonStrike.Source.Core
{
    /// <summary>
    /// Service which handles message dispatch to components
    /// </summary>
    public sealed class MessageRouter : Service
    {
        // Map from message type to the registered message handler
        private readonly Dictionary<string, DungeonStrikeComponent> _serviceMessageHandlers =
            new Dictionary<string, DungeonStrikeComponent>();

        // Map from (message type, entityId) to the registered message handler
        private readonly Dictionary<Tuple<string, string>, DungeonStrikeComponent> _entityComponentMessageHandlers =
            new Dictionary<Tuple<string, string>, DungeonStrikeComponent>();

        private readonly RingBuffer<Tuple<Message, DungeonStrikeComponent>> _messages =
            new RingBuffer<Tuple<Message, DungeonStrikeComponent>>(16);

        public void Update()
        {
            Tuple<Message, DungeonStrikeComponent> messageTarget;
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
        /// <param name="message">The newly-received message, which should be delivered to the frontend.</param>
        public void RouteMessageToFrontend(Message message)
        {
            ErrorHandler.CheckNotNull("message", message);
            var messageType = message.MessageType;
            if (message.EntityId != null)
            {
                var key = Tuple.Create(messageType, message.EntityId);
                ErrorHandler.CheckState(_entityComponentMessageHandlers.ContainsKey(key),
                    "No entity component message handler registered for message", "MessageId", message.MessageId,
                    "MessageType", message.MessageType, "EntityId", message.EntityId);
                _messages.Enqueue(Tuple.Create(message, _entityComponentMessageHandlers[key]));
            }
            else
            {
                ErrorHandler.CheckArgument(_serviceMessageHandlers.ContainsKey(messageType),
                    "No service message handler registered for message", "MessageId", message.MessageId,
                    "MessageType", message.MessageType);
                _messages.Enqueue(Tuple.Create(message, _serviceMessageHandlers[messageType]));
            }
        }
    }
}