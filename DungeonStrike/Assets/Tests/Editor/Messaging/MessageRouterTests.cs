using System;
using System.Collections.Generic;
using DungeonStrike.Assets.Source.Core;
using DungeonStrike.Assets.Source.Messaging;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DungeonStrike.Assets.Tests.Editor.Messaging
{
    class TestMessageReceiver : DungeonStrikeBehavior
    {
        public Message ReceivedMessage { get; private set; }
        public IList<string> MessageTypes { get; set; }
        public Action OnComplete { get; private set; }

        protected override IList<string> SupportedMessageTypes
        {
            get { return MessageTypes; }
        }

        protected override void HandleMessage(Message receivedMessage, Action onComplete)
        {
            ReceivedMessage = receivedMessage;
            OnComplete = onComplete;
        }
    }

    [TestFixture]
    [Category("Messaging")]
    public class MessageRouterTests : DungeonStrikeTest
    {
        private MessageRouter _messageRouter;
        private Message _testMessage1;

        [SetUp]
        public void SetUp()
        {
            _messageRouter = GetSingleton<MessageRouter>();
            _testMessage1 = new LoadSceneMessage()
            {
                MessageType = "LoadScene",
                MessageId = "123",
                GameVersion = "0.0",
                SceneName = "Test"
            };
        }

        [Test]
        public void TestReceiveMessage()
        {
            var receiver = PopulateComponent<TestMessageReceiver>(component =>
            {
                component.MessageTypes = new List<string> {_testMessage1.MessageType};
            });

            Assert.IsFalse(receiver.CurrentMessageId.HasValue);
            _messageRouter.RouteMessageToFrontend(_testMessage1);
            Assert.IsTrue(receiver.CurrentMessageId.HasValue);
            Assert.AreEqual(receiver.ReceivedMessage, _testMessage1);
            receiver.OnComplete();
            Assert.IsFalse(receiver.CurrentMessageId.HasValue);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void TestNoHandlerRegistered()
        {
            _messageRouter.RouteMessageToFrontend(_testMessage1);
        }
    }
}