﻿using System.Collections.Generic;
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
        private readonly Dictionary<string, Service> _serviceMessageHandlers = new Dictionary<string, Service>();

        // Map from (message type, entityId) to the registered message handler
        private readonly Dictionary<Tuple<string, string>, EntityComponent> _entityComponentMessageHandlers =
            new Dictionary<Tuple<string, string>, EntityComponent>();

        /// <summary>
        /// Register a <see cref="Service"/> to be the exclusive receiver of a specific type of messages
        /// </summary>
        /// <param name="messageType">The specific type of messages to register for</param>
        /// <param name="service">The service object to call when such a message is received</param>
        public void RegisterServiceForMessage(string messageType, Service service)
        {
            ErrorHandler.CheckArgumentsNotNull(new {messageType, service});
            ErrorHandler.CheckArgument(!_serviceMessageHandlers.ContainsKey(messageType),
                "Handler already registered for message", new {messageType, service});
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
            ErrorHandler.CheckArgumentsNotNull(new {messageType, entityId, component});
            var key = Tuple.Create(messageType, entityId);
            ErrorHandler.CheckArgument(!_entityComponentMessageHandlers.ContainsKey(key),
                "Handler already registered for entity message", new {messageType, entityId, component});
            _entityComponentMessageHandlers[key] = component;
        }

        /// <summary>
        /// Called when new messages are received from the driver. Should not be invoked in user code.
        /// </summary>
        /// <param name="message">The newly-received message, which should be delivered to the frontend.</param>
        public void RouteMessageToFrontend(Message message)
        {
            ErrorHandler.CheckArgumentsNotNull(new {message});
            var messageType = message.MessageType;
            if (message.EntityId != null)
            {
                var key = Tuple.Create(messageType, message.EntityId);
                ErrorHandler.CheckState(_entityComponentMessageHandlers.ContainsKey(key),
                    "No entity component message handler registered for message", new {message});
                _entityComponentMessageHandlers[key].HandleMessageFromDriver(message);
            }
            else
            {
                ErrorHandler.CheckArgument(_serviceMessageHandlers.ContainsKey(messageType),
                    "No service message handler registered for message", new {messageType});
                _serviceMessageHandlers[messageType].HandleMessageFromDriver(message);
            }
        }
    }
}