using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DisruptorUnity3d;
using DungeonStrike.Source.Messaging;
using DungeonStrike.Source.Utilities;
using Newtonsoft.Json;

namespace DungeonStrike.Source.Core
{
    /// <summary>
    /// Service which handles message dispatch to components
    /// </summary>
    public sealed class MessageRouter : Service
    {
        // Map from message type to the registered message handler
        private Dictionary<string, DungeonStrikeComponent> _serviceMessageHandlers;

        private RingBuffer<Exception> _errors;

        private RingBuffer<Tuple<Message, DungeonStrikeComponent>> _messages;

        protected override Task<Result> OnEnableService()
        {
            _serviceMessageHandlers = new Dictionary<string, DungeonStrikeComponent>();
            _errors = new RingBuffer<Exception>(16);
            _messages = new RingBuffer<Tuple<Message, DungeonStrikeComponent>>(16);
            return Async.Success;
        }

        protected override void OnDisableService()
        {
            _serviceMessageHandlers = null;
            _errors = null;
            _messages = null;
        }

        public async void Update()
        {
            Exception error;
            if (_errors.TryDequeue(out error))
            {
                throw error;
            }

            Tuple<Message, DungeonStrikeComponent> messageTarget;
            if (_messages.TryDequeue(out messageTarget))
            {
                await messageTarget.Item2.HandleMessageFromDriver(messageTarget.Item1);
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
        /// Called when new messages are received from the driver. Should not be invoked in user code.
        /// </summary>
        /// <param name="messageInput">The newly-received message JSON string, which should be delivered to the
        /// frontend.</param>
        public void RouteMessageToFrontend(string messageInput)
        {
            try
            {
                var message = JsonConvert.DeserializeObject<Message>(messageInput, Messages.GetJsonConverters());
                ErrorHandler.CheckNotNull("message", message);
                var messageType = message.MessageType;
                ErrorHandler.CheckArgument(_serviceMessageHandlers.ContainsKey(messageType),
                    "No service message handler registered for message", "MessageId", message.MessageId,
                    "MessageType", message.MessageType);
                _messages.Enqueue(Tuple.Create(message, _serviceMessageHandlers[messageType]));
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