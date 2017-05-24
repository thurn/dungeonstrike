using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DungeonStrike.Source.Core;
using DungeonStrike.Source.Utilities;
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

        protected override Task OnEnableService()
        {
            _messageRouter = GetService<MessageRouter>();
            _websocket = new WebSocket("ws://localhost:" + GetPort() + "?client-id=client");
            _websocket.OnOpen += OnOpen;
            _websocket.OnError += OnError;
            _websocket.OnClose += OnClosed;
            _websocket.OnMessage += OnMessageReceived;
            _websocket.Connect();
            _autoReconnect = AutoReconnect();
            StartCoroutine(_autoReconnect);
            return Async.Done;
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

        /// <returns>The port on which to listen for websocket connections, based on command-line arguments.</returns>
        private static string GetPort()
        {
            var args = Environment.GetCommandLineArgs();
            for (var i = 1; i < args.Length; ++i)
            {
                if (args[i - 1] == "--port")
                {
                    return args[i];
                }
            }
            return "59005";
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
            // ReSharper disable once IteratorNeverReturns
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