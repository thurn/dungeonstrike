using System;
using DungeonStrike.Source.Core;
using NUnit.Framework;
using UnityEngine;

namespace DungeonStrike.Tests.Editor.Core
{
    public class MyService : Service
    {
    }

    public class MyComponent : EntityComponent
    {
        public MyService GetMyService()
        {
            return GetService<MyService>();
        }
    }

    [TestFixture]
    [Category("Core")]
    public class DungeonStrikeBehaviorTests : DungeonStrikeTest
    {
        [Test]
        public void TestGetService()
        {
            var root = NewTestGameObject("Root");
            var rootComponent = root.AddComponent<Root>();
            var myService = root.AddComponent<MyService>();
            var obj = NewTestGameObject("Object");
            var myComponent = obj.AddComponent<MyComponent>();
            myComponent.RootObjectForTests = rootComponent;
            Assert.AreEqual(myComponent.GetMyService(), myService);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestServiceNotFound()
        {
            var root = NewTestGameObject("Root");
            var rootComponent = root.AddComponent<Root>();
            var obj = NewTestGameObject("Object");
            var myComponent = obj.AddComponent<MyComponent>();
            myComponent.RootObjectForTests = rootComponent;
            var _ = myComponent.GetMyService();
            Debug.Log("Got '" + _ + "'");
        }
    }
}