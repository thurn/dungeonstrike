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
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestOptionalNoValueException()
        {
            var empty = Optional<string>.Empty;
            var value = empty.Value;
            Assert.Fail("Value should have thrown an exception: " + value);
        }

        [Test]
        public void TestDifferentEmptiesNotEqual()
        {
            Assert.AreNotEqual(Optional<int>.Empty, Optional<string>.Empty);
        }
    }
}