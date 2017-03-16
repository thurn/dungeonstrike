using DungeonStrike.Source.Core;
using Newtonsoft.Json;
using UnityEngine;
using WebSocketSharp;

namespace DungeonStrike.Source.Messaging
{
    public sealed class WebsocketManager : Service
    {
        private MessageRouter _messageRouter;
        private WebSocket _websocket;

        protected override void OnEnable()
        {
            _messageRouter = GetService<MessageRouter>();
            _websocket = new WebSocket("ws://localhost:59005");
            _websocket.OnOpen += (sender, eventArgs) => { Logger.Log("websockets", "Connection established"); };
            _websocket.OnError += (sender, args) => { Logger.Log("websockets", "WebSocketError ", new {args}); };
            _websocket.OnClose += (sender, args) => { Debug.Log("DSCLOSE"); };
            _websocket.OnMessage += OnMessageReceived;
            _websocket.Connect();
        }

        protected override void OnDisable()
        {
            if (_websocket != null)
            {
                _websocket.Close();
            }
        }

        private void OnMessageReceived(object sender, MessageEventArgs messageArgs)
        {
            var message = JsonConvert.DeserializeObject<Message>(messageArgs.Data, new MessageConverter());
            Logger.Log("websockets", "Got Message " + message.MessageId);
            _messageRouter.RouteMessageToFrontend(message);
        }
    }
}