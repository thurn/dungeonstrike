using System;
using DungeonStrike.Source.Utilities;
using NUnit.Framework;

namespace DungeonStrike.Tests.Editor.Utilities
{
    [TestFixture]
    [Category("Core")]
    public class CompleterTests
    {
        private bool _completed;

        [Test]
        public void TestComplete()
        {
            Action action = () =>
            {
                _completed = true;
            };
            var completer = new Completer(action, "one", "two");
            completer.Complete("one");
            Assert.IsFalse(_completed);
            completer.Complete("two");
            Assert.IsTrue(_completed);
        }
    }
}