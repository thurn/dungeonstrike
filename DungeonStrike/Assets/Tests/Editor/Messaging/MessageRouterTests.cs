using DungeonStrike.Assets.Source.Core;
using NUnit.Framework;

namespace DungeonStrike.Assets.Source.Messaging
{
    [TestFixture]
    [Category("Messaging")]
    class MessageRouterTests {
        [Test]
        void TestNoHandlerReigstered()
        {
            var messageRouter = Root.Instance.GetComponent<MessageRouter>();
            messageRouter.RouteMessageToFrontend(null);
        }
    }
}
