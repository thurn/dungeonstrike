using System.Threading.Tasks;
using DungeonStrike.Source.Core;
using DungeonStrike.Source.Messaging;
using UnityEngine.SceneManagement;

namespace DungeonStrike.Source.Services
{
    /// <summary>
    /// Handles loading new game scenes.
    /// </summary>
    public sealed class SceneLoader : Service
    {
        protected override string MessageType => LoadSceneMessage.Type;

        protected override async Task HandleMessage(Message receivedMessage)
        {
            var message = (LoadSceneMessage) receivedMessage;
            Logger.Log("Loading scene", message.SceneName);
            await RunOperationAsync(SceneManager.LoadSceneAsync(message.SceneName.ToString()));
        }
    }
}