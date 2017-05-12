using System;
using DungeonStrike.Source.Core;
using DungeonStrike.Source.Messaging;
using NUnit.Framework;

namespace DungeonStrike.Tests.Editor.Core
{
    public class TestEntityComponentMessageReceiver : EntityComponent
    {
        public Message ReceivedMessage { get; private set; }
        public string TestMessageType { get; set; }
        public Action OnComplete { get; private set; }

        protected override string MessageType
        {
            get { return TestMessageType; }
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
        public string TestMessageType { get; set; }
        public Action OnComplete { get; private set; }

        protected override string MessageType
        {
            get { return TestMessageType; }
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
            _testMessage1 = new TestMessage()
            {
                MessageType = "Test",
                MessageId = "123",
                SceneName = SceneName.Empty
            };
            _testMessage2 = new TestMessage()
            {
                MessageType = "Test",
                MessageId = "123",
                EntityId = "321",
                SceneName = SceneName.Empty
            };
            _testMessage3 = new TestMessage()
            {
                MessageType = "Test",
                MessageId = "789",
                SceneName = SceneName.Empty
            };
        }

        [Test]
        public void TestReceiveMessage()
        {
            var receiver = CreateTestService<TestServiceMessageReceiver>();
            receiver.TestMessageType = _testMessage1.MessageType;

            EnableObjects();

            Assert.IsFalse(receiver.CurrentMessageId.HasValue);
            _messageRouter.RouteMessageToFrontend(_testMessage1.ToJson());
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
            receiver1.TestMessageType = _testMessage2.MessageType;

            var testEntity2 = NewTestEntityObject("entityObject", "entityId", "SomeOtherEntityId");
            var receiver2 = AddTestEntityComponent<TestEntityComponentMessageReceiver>(testEntity2);
            receiver2.TestMessageType = _testMessage2.MessageType;

            EnableObjects();

            Assert.IsFalse(receiver1.CurrentMessageId.HasValue);
            Assert.IsFalse(receiver2.CurrentMessageId.HasValue);
            _messageRouter.RouteMessageToFrontend(_testMessage2.ToJson());
            _messageRouter.Update();
            Assert.IsTrue(receiver1.CurrentMessageId.HasValue);
            Assert.IsFalse(receiver2.CurrentMessageId.HasValue);
            Assert.AreEqual(receiver1.ReceivedMessage, _testMessage2);
            receiver1.OnComplete();
            Assert.IsFalse(receiver1.CurrentMessageId.HasValue);
        }

        [Test]
        public void TestAlreadyHandlingMessage()
        {
            var receiver = CreateTestService<TestServiceMessageReceiver>();
            receiver.TestMessageType = _testMessage1.MessageType;
            EnableObjects();
            Assert.IsFalse(receiver.CurrentMessageId.HasValue);
            _messageRouter.RouteMessageToFrontend(_testMessage1.ToJson());
            _messageRouter.Update();
            _messageRouter.RouteMessageToFrontend(_testMessage3.ToJson());
            Assert.Throws<InvalidOperationException>(delegate { _messageRouter.Update(); });
        }

        [Test]
        public void TestTwoHandlersRegistered()
        {
            var receiver1 = CreateTestService<TestServiceMessageReceiver>();
            receiver1.TestMessageType = _testMessage1.MessageType;
            var receiver2 = CreateTestService<TestServiceMessageReceiver>();
            receiver2.TestMessageType = _testMessage1.MessageType;

            Assert.Throws<ArgumentException>(EnableObjects);
        }

        [Test]
        public void TestTwoEntityIdHandlersRegistered()
        {
            var testEntity1 = NewTestEntityObject("entityObject", "entityId", _testMessage2.EntityId);
            var receiver1 = AddTestEntityComponent<TestEntityComponentMessageReceiver>(testEntity1);
            receiver1.TestMessageType = _testMessage2.MessageType;

            var testEntity2 = NewTestEntityObject("entityObject", "entityId", _testMessage2.EntityId);
            var receiver2 = AddTestEntityComponent<TestEntityComponentMessageReceiver>(testEntity2);
            receiver2.TestMessageType = _testMessage2.MessageType;

            Assert.Throws<ArgumentException>(EnableObjects);
        }

        [Test]
        public void TestNoHandlerRegistered()
        {
            try
            {
                EnableObjects();
                _messageRouter.RouteMessageToFrontend(_testMessage1.ToJson());
                _messageRouter.Update();
                Assert.Fail("Expected exception!");
            }
            catch (ArgumentException _)
            {
                // Expected
            }
        }
    }
}