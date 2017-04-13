using System.Collections;
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
            _websocket = new WebSocket("ws://localhost:59005?client-id=client");
            _websocket.OnOpen += (sender, eventArgs) => { Logger.Log("Unity got connection"); };
            _websocket.OnError += (sender, args) => {
                Logger.Log("Unity WebSocketError", "Message", args.Message, "Exception", args.Exception);
            };
            _websocket.OnClose += (sender, args) => { Logger.Log("Unity connection closed"); };
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

        public void ToCreate2()
        {
            _messageRouter.RouteMessageToFrontend(null);
        }

        protected override void Start()
        {
            //StartCoroutine(SendMessage());
        }

        private IEnumerator SendMessage()
        {
            yield return new WaitForSeconds(4.0f);
            _websocket.SendAsync("Hello, world", success => Logger.Log("Message sent"));
        }

        private void OnMessageReceived(object sender, MessageEventArgs messageArgs)
        {
            //var message = JsonConvert.DeserializeObject<Message>(messageArgs.Data, new MessageConverter());
            Logger.Log("Got Message " + messageArgs.Data);
            //_messageRouter.RouteMessageToFrontend(message);
        }
    }
}
