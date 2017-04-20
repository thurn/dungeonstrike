using DungeonStrike.Source.Messaging;
using Newtonsoft.Json;
using NUnit.Framework;

namespace DungeonStrike.Tests.Editor.Messaging
{
    [TestFixture]
    [Category("Messaging")]
    public class MessageConverterTests : DungeonStrikeTest
    {
        private const string JsonInput = "{\"MessageType\": \"LoadScene\", " +
                                          "\"MessageId\": \"123\", " +
                                          "\"GameVersion\": \"0.1.0\", " +
                                          "\"SceneName\": \"Flat\"}";

        private readonly LoadSceneMessage _messageResult = new LoadSceneMessage
        {
            MessageType = "LoadScene",
            MessageId = "123",
            SceneName = SceneName.Flat
        };

        [Test]
        public void TestParseMessage()
        {
            var result = JsonConvert.DeserializeObject<Message>(JsonInput, new MessageConverter());
            Assert.IsInstanceOf<LoadSceneMessage>(result);
            Assert.AreEqual(_messageResult, result);
        }

        [Test]
        public void TestRoundTripMessage()
        {
            var json = JsonConvert.SerializeObject(_messageResult);
            var obj = JsonConvert.DeserializeObject<LoadSceneMessage>(json);
            Assert.AreEqual(_messageResult, obj);
            Assert.IsFalse(false);
        }
    }
}