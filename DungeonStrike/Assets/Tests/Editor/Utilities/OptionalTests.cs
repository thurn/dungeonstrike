using System;
using DungeonStrike.Source.Utilities;
using NUnit.Framework;

namespace DungeonStrike.Tests.Editor.Utilities
{
    [TestFixture]
    [Category("Core")]
    public class OptionalTests
    {
        [Test]
        public void TestOptional()
        {
            var optional = Optional.Of("foo");
            Assert.IsTrue(optional.HasValue);
            var other = Optional.Of("foo");
            Assert.AreEqual(optional, other);
            var empty = Optional<string>.Empty;
            Assert.IsFalse(empty.HasValue);
            Assert.AreNotEqual(optional, empty);
            Assert.AreEqual(empty, Optional<string>.Empty);
        }

        [Test]
        public void TestOptionalNoValueException()
        {
            var empty = Optional<string>.Empty;
            Assert.Throws<InvalidOperationException>(delegate { var _ = empty.Value; });
        }

        [Test]
        public void TestDifferentEmptiesNotEqual()
        {
            Assert.AreNotEqual(Optional<int>.Empty, Optional<string>.Empty);
        }
    }
}