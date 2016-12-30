using System;
using System.Collections.Generic;
using DungeonStrike.Assets.Source.Core;
using DungeonStrike.Assets.Source.Messaging;
using UnityEngine.SceneManagement;

namespace DungeonStrike.Assets.Source.Services
{
    public class SceneLoader : DungeonStrikeBehavior
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