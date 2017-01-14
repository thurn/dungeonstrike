using System;
using System.Diagnostics;
using DungeonStrike.Source.Core;
using Newtonsoft.Json;
using WebSocketSharp;

namespace DungeonStrike.Source.Messaging
{
    public sealed class WebsocketManager : Service
    {
        private MessageRouter _messageRouter;
        private WebSocket _websocket;

        protected override void Start()
        {
            _messageRouter = GetService<MessageRouter>();
            _websocket = new WebSocket("ws://localhost:59005");
            _websocket.OnOpen += (sender, eventArgs) =>
            {
                Logger.Log("Connection established");
            };
            _websocket.OnError += (sender, args) =>
            {
                Logger.Log("WebSocketError");
            };
            _websocket.OnMessage += OnMessageReceived;
            Logger.Log("Connecting to web socket " + _websocket);
            _websocket.Connect();
        }

        private void OnMessageReceived(object sender, MessageEventArgs messageArgs)
        {
            Logger.Log("Got Message " + messageArgs.Data);
            _websocket.Send("{\"hello\": \"clojure\"}");
            return;
            var message = JsonConvert.DeserializeObject<Message>(messageArgs.Data, new MessageConverter());
            _messageRouter.RouteMessageToFrontend(message);
        }

        protected override void OnDisable()
        {
            if (_websocket != null)
            {
                _websocket.Close();
            }
        }
    }
}