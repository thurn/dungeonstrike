using DungeonStrike.Source.Core;
using Newtonsoft.Json;
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
            _websocket.OnOpen += (sender, eventArgs) => { Logger.Log("Connection established"); };
            _websocket.OnError += (sender, args) => { Logger.Log("WebSocketError ", new {args}); };
            _websocket.OnClose += (sender, args) => { Logger.Log("Connection closed"); };
            _websocket.OnMessage += OnMessageReceived;
            Logger.Log("Connecting to web socket " + _websocket);
            _websocket.Connect();
        }

        protected override void OnDisable()
        {
            Logger.Log("Closing websocket connection");
            if (_websocket != null)
            {
                _websocket.Close();
            }
        }

        private void OnMessageReceived(object sender, MessageEventArgs messageArgs)
        {
            var message = JsonConvert.DeserializeObject<Message>(messageArgs.Data, new MessageConverter());
            Logger.Log("Got Message " + message.MessageId);
            _messageRouter.RouteMessageToFrontend(message);
        }
    }
}