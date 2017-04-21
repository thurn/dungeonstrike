using System;
using System.Collections.Generic;
using DungeonStrike.Source.Core;
using UnityEngine;
using WebSocketSharp;

namespace DungeonStrike.Source.Messaging
{
    public sealed class WebsocketManager : Service
    {
        private MessageRouter _messageRouter;
        private WebSocket _websocket;
        private IEnumerator<WaitForSeconds> _autoReconnect;

        protected override void OnEnableService()
        {
            _messageRouter = GetService<MessageRouter>();
            _websocket = new WebSocket("ws://localhost:59005?client-id=client");
            _websocket.OnOpen += OnOpen;
            _websocket.OnError += OnError;
            _websocket.OnClose += OnClosed;
            _websocket.OnMessage += OnMessageReceived;
            _websocket.Connect();
            _autoReconnect = AutoReconnect();
            StartCoroutine(_autoReconnect);
        }

        protected override void OnDisableService()
        {
            StopCoroutine(_autoReconnect);
            if (_websocket != null)
            {
                _websocket.OnOpen -= OnOpen;
                _websocket.OnError -= OnError;
                _websocket.OnClose -= OnClosed;
                _websocket.OnMessage -= OnMessageReceived;
                _websocket.Close();
                _websocket = null;
            }
        }

        private IEnumerator<WaitForSeconds> AutoReconnect()
        {
            while (true)
            {
                yield return new WaitForSeconds(1.0f);
                if (!_websocket.IsConnected)
                {
                    _websocket.Connect();
                }
            }
        }

        private void OnOpen(object sender, EventArgs args)
        {
            Logger.Log("Unity got connection");
        }

        private void OnError(object sender, ErrorEventArgs args)
        {
            Logger.Log("Unity WebSocketError", "Message", args.Message, "Exception", args.Exception);
        }

        private void OnClosed(object sender, CloseEventArgs args)
        {
            Logger.Log("Unity connection closed");
        }

        private void OnMessageReceived(object sender, MessageEventArgs messageArgs)
        {
            Logger.Log("Unity got message", messageArgs.Data.Substring(0, 25) + "...");
            _messageRouter.RouteMessageToFrontend(messageArgs.Data);
        }
    }
}
