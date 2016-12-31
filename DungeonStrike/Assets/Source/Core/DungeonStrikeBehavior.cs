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

        private ErrorHandler _errorHandler;

        protected ErrorHandler ErrorHandler
        {
            get { return _errorHandler ?? (_errorHandler = new ErrorHandler(this)); }
        }

        private Root _root;

        public Root RootObjectForTests
        {
            get { return _root; }
            set
            {
                if (_root != null)
                {
                    throw new InvalidOperationException("Cannot override existing root object.");
                }
                _root = value;
            }
        }

        protected T GetService<T>() where T : Component
        {
            if (_root != null) return _root.GetComponent<T>();
            var roots = FindObjectsOfType<Root>();
            if (roots.Length != 1)
            {
                throw new InvalidOperationException("Exactly one Root object must be created.");
            }
            _root = roots[0];
            var result = _root.GetComponent<T>();
            ErrorHandler.CheckState(result != null, "Unable to locate service " + typeof(T));
            return result;
        }

        private MessageRouter _messageRouter;

        protected MessageRouter MessageRouter
        {
            get { return _messageRouter ?? (_messageRouter = GetService<MessageRouter>()); }
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

            DungeonStrikeBehaviorAwake();
        }

        public virtual void DungeonStrikeBehaviorAwake()
        {
        }

        public virtual void Start()
        {
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
                () => new {CurrentMessageId.Value, message});
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