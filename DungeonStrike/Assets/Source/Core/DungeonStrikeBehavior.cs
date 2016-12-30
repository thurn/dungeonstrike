using System;
using System.Collections.Generic;
using DungeonStrike.Assets.Source.Messaging;
using UnityEngine;

namespace DungeonStrike.Assets.Source.Core
{
    public abstract class DungeonStrikeBehavior : MonoBehaviour
    {
        private ILogger _logger;

        protected ILogger Logger
        {
            get { return _logger ?? (_logger = new StandardLogger(this)); }
        }

        private IErrorHandler _errorHandler;

        protected IErrorHandler ErrorHandler
        {
            get { return _errorHandler ?? (_errorHandler = new StandardErrorHandler(this)); }
        }

        protected T GetSingleton<T>() where T : Component
        {
            return Root.Instance.GetComponent<T>();
        }

        public int MyInt()
        {
            return 4;
        }

        private MessageRouter _messageRouter;

        protected MessageRouter MessageRouter
        {
            get { return _messageRouter ?? (_messageRouter = GetSingleton<MessageRouter>()); }
        }

        public void Awake()
        {
            var entity = GetComponent<Entity>();
            if (entity)
            {
                var entityId = entity.EntityId;
                foreach (var messageType in SupportedMessageTypes)
                {
                    MessageRouter.RegisterForEntityMessage(messageType, entityId, this);
                }
            }
            else
            {
                foreach (var messageType in SupportedMessageTypes)
                {
                    MessageRouter.RegisterForMessage(messageType, this);
                }
            }
        }

        public Optional<string> CurrentMessageId { get; private set; }

        protected virtual IList<string> SupportedMessageTypes
        {
            get { return new List<string>(); }
        }

        public void HandleMessageFromDriver(Message message)
        {
            Logger.Log("Received message", new {message});
            ErrorHandler.CheckState(!CurrentMessageId.HasValue, "Component is already handling a message",
                new {CurrentMessageId.Value, message});
            CurrentMessageId = Optional<string>.Of(message.MessageId);
            HandleMessage(message, () =>
            {
                Logger.Log("Finished processing message", new {message});
                CurrentMessageId = Optional<string>.Empty;
            });
        }

        protected virtual void HandleMessage(Message receivedMessage, Action onComplete)
        {
        }
    }
}