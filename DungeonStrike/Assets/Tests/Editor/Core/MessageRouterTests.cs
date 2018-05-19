using System;
using System.Threading.Tasks;
using DungeonStrike.Source.Core;
using DungeonStrike.Source.Messaging;
using DungeonStrike.Source.Utilities;
using NUnit.Framework;
using UnityEngine;

namespace DungeonStrike.Tests.Editor.Core
{
    public class TestServiceMessageReceiver : Service
    {
        public Message ReceivedMessage { get; private set; }
        public string TestMessageType { get; set; }
        public TaskCompletionSource<bool> CompletionSource { get; } = new TaskCompletionSource<bool>();

        protected override string MessageType => TestMessageType;

        protected override async Task<Result> HandleMessage(Message receivedMessage)
        {
            ReceivedMessage = receivedMessage;
            await CompletionSource.Task;
            return Result.Success;
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
            _testMessage3 = new TestMessage()
            {
                MessageType = "Test",
                MessageId = "789",
                SceneName = SceneName.Empty
            };
        }

        [Test]
        public async void TestReceiveMessage()
        {
            var receiver = CreateTestService<TestServiceMessageReceiver>();
            receiver.TestMessageType = _testMessage1.MessageType;

            await EnableObjects();

            Assert.IsFalse(receiver.CurrentMessageId.HasValue);
            _messageRouter.RouteMessageToFrontend(_testMessage1.ToJson());
            _messageRouter.Update();
            Assert.IsTrue(receiver.CurrentMessageId.HasValue);
            Assert.AreEqual(receiver.ReceivedMessage, _testMessage1);
            receiver.CompletionSource.SetResult(true);
            Assert.IsFalse(receiver.CurrentMessageId.HasValue);
        }

        [Test]
        public async void TestAlreadyHandlingMessage()
        {
            var receiver = CreateTestService<TestServiceMessageReceiver>();
            receiver.TestMessageType = _testMessage1.MessageType;
            await EnableObjects();
            Assert.IsFalse(receiver.CurrentMessageId.HasValue);
            try
            {
                await receiver.HandleMessageFromDriver(_testMessage1);
                await receiver.HandleMessageFromDriver(_testMessage3);
                Debug.Log("Failing");
                Assert.Fail("Expected exception!");
            }
            catch (InvalidOperationException)
            {
                // Expected
            }
        }

        [Test]
        public async void TestTwoHandlersRegistered()
        {
            var receiver1 = CreateTestService<TestServiceMessageReceiver>();
            receiver1.TestMessageType = _testMessage1.MessageType;
            var receiver2 = CreateTestService<TestServiceMessageReceiver>();
            receiver2.TestMessageType = _testMessage1.MessageType;

            try
            {
                await EnableObjects();
            }
            catch (ArgumentException)
            {
                // Expected
            }
        }

        [Test]
        public async void TestNoHandlerRegistered()
        {
            try
            {
                await EnableObjects();
                _messageRouter.RouteMessageToFrontend(_testMessage1.ToJson());
                _messageRouter.Update();
                Assert.Fail("Expected exception!");
            }
            catch (ArgumentException)
            {
                // Expected
            }
        }
    }
}