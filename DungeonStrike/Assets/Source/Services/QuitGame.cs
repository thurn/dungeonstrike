using System;
using DungeonStrike.Source.Core;
using DungeonStrike.Source.Messaging;
using UnityEngine;

namespace DungeonStrike.Source.Services
{
    /// <summary>
    /// Handles quitting the game.
    /// </summary>
    public sealed class QuitGame : Service
    {
        protected override string MessageType
        {
            get { return QuitGameMessage.Type; }
        }

        protected override void HandleMessage(Message receivedMessage, Action onComplete)
        {
            Logger.Log("Quitting Client");
            Application.Quit();
        }
    }
}