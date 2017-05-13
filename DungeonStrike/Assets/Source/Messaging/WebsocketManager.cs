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
        private bool _connectionClosed;

        protected override void OnEnableService(Action onStart)
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
            onStart();
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
                if (_connectionClosed && (_websocket.ReadyState == WebSocketState.Closed))
                {
                    _websocket.Connect();
                }
            }
        }

        private void OnOpen(object sender, EventArgs args)
        {
            _connectionClosed = false;
            Root.RunWhenReady(() => Logger.Log(">> client connected <<"));
        }

        private void OnError(object sender, ErrorEventArgs args)
        {
            Logger.Log("Unity WebSocketError", "Message", args.Message, "Exception", args.Exception);
        }

        private void OnClosed(object sender, CloseEventArgs args)
        {
            if (!_connectionClosed)
            {
                Logger.Log("Unity connection closed");
            }
            _connectionClosed = true;
        }

        private void OnMessageReceived(object sender, MessageEventArgs messageArgs)
        {
            _messageRouter.RouteMessageToFrontend(messageArgs.Data);
        }
    }
}
