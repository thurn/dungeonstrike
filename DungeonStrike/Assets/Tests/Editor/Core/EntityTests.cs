using DungeonStrike.Source.Core;
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
            Assert.IsNull(entity.EntityType);
            Assert.IsNull(entity.EntityId);
            Assert.IsFalse(entity.Initialized);
            entity.Initialize("entityType", "entityId");
            Assert.AreEqual("entityType", entity.EntityType);
            Assert.AreEqual("entityId", entity.EntityId);
            Assert.IsTrue(entity.Initialized);
        }
    }
}