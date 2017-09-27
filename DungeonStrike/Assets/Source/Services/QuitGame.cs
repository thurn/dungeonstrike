using System.Threading.Tasks;
using DungeonStrike.Source.Core;
using DungeonStrike.Source.Messaging;
using DungeonStrike.Source.Utilities;
using UnityEngine;

namespace DungeonStrike.Source.Services
{
    /// <summary>
    /// Handles quitting the game.
    /// </summary>
    public sealed class QuitGame : Service
    {
        protected override string MessageType => QuitGameMessage.Type;

        protected override Task<Result> HandleMessage(Message receivedMessage)
        {
            Logger.Log("Quitting Client");
            Application.Quit();
            return Async.Success;
        }
    }
}