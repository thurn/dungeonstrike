﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using DungeonStrike.Source.Core;
using DungeonStrike.Source.Messaging;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DungeonStrike.Source.Services
{
    public sealed class SceneLoader : Service
    {
        protected override void OnEnable()
        {
            Logger.Log("dt", "SceneLoaderException");
            //throw new InvalidOperationException("ioe");
        }

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
            Logger.Log("scene-loader", "Loading scene " + message.SceneName);
            yield return SceneManager.LoadSceneAsync(message.SceneName);
            onComplete();
        }
    }
}