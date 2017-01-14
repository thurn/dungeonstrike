using System;
using System.Collections.Generic;
using DungeonStrike.Source.Core;
using DungeonStrike.Source.Messaging;
using UnityEngine.SceneManagement;

namespace DungeonStrike.Source.Services
{
    public sealed class SceneLoader : Service
    {
        protected override IList<string> SupportedMessageTypes
        {
            get { return new List<string> {"LoadScene"}; }
        }

        protected override void HandleMessage(Message receivedMessage, Action onComplete)
        {
            var message = (LoadSceneMessage) receivedMessage;
            SceneManager.LoadSceneAsync(message.SceneName);
        }
    }
}