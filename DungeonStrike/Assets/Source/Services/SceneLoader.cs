using System;
using System.Collections.Generic;
using DungeonStrike.Source.Core;
using DungeonStrike.Source.Messaging;
using UnityEngine;

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
            StartCoroutine(LoadSceneAsync(message, onComplete));
        }

        private IEnumerator<YieldInstruction> LoadSceneAsync(LoadSceneMessage message, Action onComplete)
        {
            Logger.Log("Loading scenxe", message.SceneName);
            //yield return SceneManager.LoadSceneAsync(message.SceneName.ToString());
            onComplete();
            yield return null;
        }
    }
}