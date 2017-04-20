using System;
using System.Collections.Generic;
using DungeonStrike.Source.Core;
using DungeonStrike.Source.Messaging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DungeonStrike.Source.Services
{
    public sealed class SceneLoader : Service
    {
        protected override IList<string> SupportedMessageTypes
        {
            get { return new List<string> {"LoadScene"}; }
        }

//        protected override void Start()
//        {
//            var loadScene = new LoadSceneMessage
//            {
//                MessageId = "123",
//                MessageType = "LoadScene",
//                SceneName = "Flat",
//                Position = new Position {X = 1, Y = 2},
//                EntityType = EntityType.Orc
//            };
//            var settings = new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore};
//            settings.Converters.Add(new StringEnumConverter());
//            Debug.Log(JsonConvert.SerializeObject(loadScene, settings));
//            var msg =
//                "{\"SceneName\":\"Flat\",\"Position\":{\"X\":1,\"Y\":2},\"EntityType\":\"Orc\",\"MessageId\":\"123\",\"MessageType\":\"LoadScene\"}";
//            var message = JsonConvert.DeserializeObject<Message>(msg, new MessageConverter(), new StringEnumConverter());
//            var ls = message as LoadSceneMessage;
//            Debug.Log("name " + ls.SceneName + " et " + ls.EntityType + " pos " + ls.Position.X);
//        }

        protected override void HandleMessage(Message receivedMessage, Action onComplete)
        {
            var message = (LoadSceneMessage) receivedMessage;
            StartCoroutine(LoadSceneAsync(message, onComplete));
        }

        private IEnumerator<YieldInstruction> LoadSceneAsync(LoadSceneMessage message, Action onComplete)
        {
            Logger.Log("Loading scene", message.SceneName);
            yield return SceneManager.LoadSceneAsync(message.SceneName.ToString());
            onComplete();
        }
    }
}