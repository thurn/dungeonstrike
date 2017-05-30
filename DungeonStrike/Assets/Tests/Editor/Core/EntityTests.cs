using DungeonStrike.Source.Core;
using DungeonStrike.Source.Messaging;
using NUnit.Framework;

namespace DungeonStrike.Tests.Editor.Core
{
    [TestFixture]
    [Category("Core")]
    public class EntityTests : DungeonStrikeTest
    {
        [Test]
        public void TestInitializeEntity()
        {
            var obj = NewTestGameObject("entityContainer");
            var entity = obj.AddComponent<Entity>();
            Assert.IsNull(entity.EntityId);
            Assert.IsFalse(entity.Initialized);
            entity.Initialize(EntityType.Soldier, "entityId");
            Assert.AreEqual(EntityType.Soldier, entity.EntityType);
            Assert.AreEqual("entityId", entity.EntityId);
            Assert.IsTrue(entity.Initialized);
        }
    }
}