using System;
using System.Collections.Generic;

namespace DungeonStrike.Source.Utilities
{
    /// <summary>
    /// Utility class containg static methods for interacting with <see cref="Optional{T}"/> values.
    /// </summary>
    public static class Optional
    {
        /// <summary>
        /// Creates a new Optional instance wrapping a value.
        /// </summary>
        /// <param name="value">The optional's value.</param>
        /// <typeparam name="T">The type of <paramref name="value"/></typeparam>
        /// <returns>A newly created <see cref="Optional{T}"/> instance containing <paramref name="value"/></returns>
        public static Optional<T> Of<T>(T value)
        {
            return new Optional<T>(value);
        }
    }

    /// <summary>
    /// Represents a reference type that may or may not exist as an alternative to <c>null</c>.
    /// </summary>
    /// <remarks>
    /// The class implements a construct similar to the java.util.Optional class first introduced in Java 8
    /// (https://docs.oracle.com/javase/8/docs/api/java/util/Optional.html). It is intended to replace situations where
    /// a specific reference type value could have been null. It is analagous to System.Nullable for value types. You
    /// should use <see cref="Optional.Of{T}" /> to construct Optional instances.
    /// </remarks>
    /// <typeparam name="T">The type of the optional value</typeparam>
    public struct Optional<T> : IEquatable<Optional<T>>
    {
        // Value field
        private readonly T _value;
        // Whether or not the value has been specified.
        private readonly bool _hasValue;

        /// <summary>
        /// The empty <c>Optional</c>, representing the absence of a reference value.
        /// </summary>
        public static Optional<T> Empty { get; private set; }

        /// <summary>
        /// Returns true if an explicit value has been set, false if this is <see cref="Optional{T}.Empty" />.
        /// </summary>
        public bool HasValue
        {
            get { return _hasValue; }
        }

        /// <summary>
        /// Retrieves the value of this optional, if it has one.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if this method is invoked on an
        /// <see cref="Optional{T}.Empty" /> object.
        /// </exception>
        public T Value
        {
            get
            {
                if (!_hasValue) throw new InvalidOperationException("Optional does not have a value.");
                return _value;
            }
        }

        /// <summary>
        /// Constructs a new optional instance. Prefer <see cref="Optional.Of{T}" /> over calling this directly.
        /// </summary>
        /// <param name="value">The value to wrap.</param>
        public Optional(T value)
        {
            _hasValue = true;
            _value = value;
        }

        // Initializes the empty value.
        static Optional()
        {
            Empty = new Optional<T>();
        }

        /// <see cref="Object.ToString" />
        public override string ToString()
        {
            return _hasValue ? _value.ToString() : "[Optional.Empty]";
        }

        /// <see cref="Object.Equals(Object)" />
        public static bool operator ==(Optional<T> lhs, Optional<T> rhs)
        {
            return lhs.Equals(rhs);
        }

        /// <see cref="Object.Equals(Object)" />
        public static bool operator !=(Optional<T> lhs, Optional<T> rhs)
        {
            return !lhs.Equals(rhs);
        }

        /// <see cref="Object.Equals(Object)" />
        public bool Equals(Optional<T> other)
        {
            if (_hasValue != other.HasValue) return false;
            return !_hasValue || EqualityComparer<T>.Default.Equals(_value, other.Value);
        }

        /// <see cref="Object.Equals(Object)" />
        public override bool Equals(object obj)
        {
            if (obj is Optional<T>) return Equals((Optional<T>)obj);
            return false;
        }

        /// <see cref="Object.GetHashCode()" />
        public override int GetHashCode()
        {
            return _hasValue ? EqualityComparer<T>.Default.GetHashCode(_value) : 0;
        }
    }
}
