using System.Collections.Generic;
using DungeonStrike.Assets.Source.Core;

namespace DungeonStrike.Assets.Source.Messaging
{
    public sealed class MessageRouter : DungeonStrikeBehavior
    {
        private readonly Dictionary<string, DungeonStrikeBehavior> _messageTypeHandlers =
            new Dictionary<string, DungeonStrikeBehavior>();

        private readonly Dictionary<string, Dictionary<string, DungeonStrikeBehavior>> _entityMessageHandlers =
            new Dictionary<string, Dictionary<string, DungeonStrikeBehavior>>();

        public void RegisterForMessage(string messageType, DungeonStrikeBehavior behavior)
        {
            ErrorHandler.CheckNotNull(new {messageType, behavior});
            ErrorHandler.CheckArgument(!_messageTypeHandlers.ContainsKey(messageType),
                "Handler already registered for message", () => new {messageType, behavior});
            _messageTypeHandlers[messageType] = behavior;
        }

        public void RegisterForEntityMessage(string messageType, string entityId, DungeonStrikeBehavior behavior)
        {
            if (!_entityMessageHandlers.ContainsKey(messageType))
            {
                _entityMessageHandlers[messageType] = new Dictionary<string, DungeonStrikeBehavior>();
            }
            ErrorHandler.CheckArgument(!_entityMessageHandlers[messageType].ContainsKey(entityId),
                "Handler already registered for entity message", () => new {messageType, entityId, behavior});
            _entityMessageHandlers[messageType][entityId] = behavior;
        }

        public void RouteMessageToFrontend(Message message)
        {
            ErrorHandler.CheckNotNull(new {message});
            var messageType = message.MessageType;
            if (message.EntityId != null)
            {
                ErrorHandler.CheckState(_entityMessageHandlers.ContainsKey(messageType) &&
                    _entityMessageHandlers[messageType].ContainsKey(message.EntityId),
                    "No entity handler registered for message", () => new {message});
                _entityMessageHandlers[messageType][message.EntityId].HandleMessageFromDriver(message);
            }
            else
            {
                ErrorHandler.CheckArgument(_messageTypeHandlers.ContainsKey(messageType),
                    "No handler registered for message", () => new {messageType});
                _messageTypeHandlers[messageType].HandleMessageFromDriver(message);
            }
        }

        public void SendMessageToDriver(Message message)
        {
        }
    }
}