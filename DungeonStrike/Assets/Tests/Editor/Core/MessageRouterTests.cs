using System;
using System.Collections.Generic;
using DungeonStrike.Source.Core;
using DungeonStrike.Source.Messaging;
using NUnit.Framework;

namespace DungeonStrike.Tests.Editor.Core
{
    public class TestEntityComponentMessageReceiver : EntityComponent
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

    public class TestServiceMessageReceiver : Service
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
    [Category("Core")]
    public class MessageRouterTests : DungeonStrikeTest
    {
        private MessageRouter _messageRouter;
        private Message _testMessage1;
        private Message _testMessage2;
        private Message _testMessage3;

        [SetUp]
        public void SetUp()
        {
            _messageRouter = GetService<MessageRouter>();
            _testMessage1 = new LoadSceneMessage()
            {
                MessageType = "TestMessage",
                MessageId = "123",
                SceneName = SceneName.Empty
            };
            _testMessage2 = new LoadSceneMessage()
            {
                MessageType = "TestMessage",
                MessageId = "123",
                EntityId = "321",
                SceneName = SceneName.Empty
            };
            _testMessage3 = new LoadSceneMessage()
            {
                MessageType = "TestMessage",
                MessageId = "789",
                SceneName = SceneName.Empty
            };
        }

        [Test]
        public void TestReceiveMessage()
        {
            var receiver = CreateTestService<TestServiceMessageReceiver>();
            receiver.MessageTypes = new List<string> {_testMessage1.MessageType};

            AwakeAndStartObjects();

            Assert.IsFalse(receiver.CurrentMessageId.HasValue);
            _messageRouter.RouteMessageToFrontend(_testMessage1);
            _messageRouter.Update();
            Assert.IsTrue(receiver.CurrentMessageId.HasValue);
            Assert.AreEqual(receiver.ReceivedMessage, _testMessage1);
            receiver.OnComplete();
            Assert.IsFalse(receiver.CurrentMessageId.HasValue);
        }

        [Test]
        public void TestReceiveEntityMessage()
        {
            var testEntity1 = NewTestEntityObject("entityObject", "entityId", _testMessage2.EntityId);
            var receiver1 = AddTestEntityComponent<TestEntityComponentMessageReceiver>(testEntity1);
            receiver1.MessageTypes = new List<string> {_testMessage2.MessageType};

            var testEntity2 = NewTestEntityObject("entityObject", "entityId", "SomeOtherEntityId");
            var receiver2 = AddTestEntityComponent<TestEntityComponentMessageReceiver>(testEntity2);
            receiver2.MessageTypes = new List<string> {_testMessage2.MessageType};

            AwakeAndStartObjects();

            Assert.IsFalse(receiver1.CurrentMessageId.HasValue);
            Assert.IsFalse(receiver2.CurrentMessageId.HasValue);
            _messageRouter.RouteMessageToFrontend(_testMessage2);
            _messageRouter.Update();
            Assert.IsTrue(receiver1.CurrentMessageId.HasValue);
            Assert.IsFalse(receiver2.CurrentMessageId.HasValue);
            Assert.AreEqual(receiver1.ReceivedMessage, _testMessage2);
            receiver1.OnComplete();
            Assert.IsFalse(receiver1.CurrentMessageId.HasValue);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestAlreadyHandlingMessage()
        {
            var receiver = CreateTestService<TestServiceMessageReceiver>();
            receiver.MessageTypes = new List<string> {_testMessage1.MessageType};
            AwakeAndStartObjects();
            Assert.IsFalse(receiver.CurrentMessageId.HasValue);
            _messageRouter.RouteMessageToFrontend(_testMessage1);
            _messageRouter.Update();
            _messageRouter.RouteMessageToFrontend(_testMessage3);
            _messageRouter.Update();
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void TestTwoHandlersRegistered()
        {
            var receiver1 = CreateTestService<TestServiceMessageReceiver>();
            receiver1.MessageTypes = new List<string> {_testMessage1.MessageType};
            var receiver2 = CreateTestService<TestServiceMessageReceiver>();
            receiver2.MessageTypes = new List<string> {_testMessage1.MessageType};

            AwakeAndStartObjects();
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void TestTwoEntityIdHandlersRegistered()
        {
            var testEntity1 = NewTestEntityObject("entityObject", "entityId", _testMessage2.EntityId);
            var receiver1 = AddTestEntityComponent<TestEntityComponentMessageReceiver>(testEntity1);
            receiver1.MessageTypes = new List<string> {_testMessage2.MessageType};

            var testEntity2 = NewTestEntityObject("entityObject", "entityId", _testMessage2.EntityId);
            var receiver2 = AddTestEntityComponent<TestEntityComponentMessageReceiver>(testEntity2);
            receiver2.MessageTypes = new List<string> {_testMessage2.MessageType};

            AwakeAndStartObjects();
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestNullMessageType()
        {
            var receiver = CreateTestService<TestServiceMessageReceiver>();
            receiver.MessageTypes = null;

            AwakeAndStartObjects();
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void TestNoHandlerRegistered()
        {
            _messageRouter.RouteMessageToFrontend(_testMessage1);
            _messageRouter.Update();
        }
    }
}