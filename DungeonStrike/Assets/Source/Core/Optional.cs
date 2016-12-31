using System;
using System.Collections.Generic;

namespace DungeonStrike.Assets.Source.Core
{
    public struct Optional<T> : IEquatable<Optional<T>>
    {
        private readonly T _value;
        private readonly bool _hasValue;

        public static Optional<T> Of(T value)
        {
            return new Optional<T>(value);
        }

        public static Optional<T> Empty { get; private set; }

        public bool HasValue
        {
            get { return _hasValue; }
        }

        public T Value
        {
            get
            {
                if (!_hasValue) throw new InvalidOperationException("Optional does not have a value.");
                return _value;
            }
        }

        internal Optional(T value)
        {
            _hasValue = true;
            _value = value;
        }

        static Optional()
        {
            Empty = new Optional<T>();
        }

        public override string ToString()
        {
            return _hasValue ? _value.ToString() : "[Optional.Empty]";
        }

        public static bool operator ==(Optional<T> lhs, Optional<T> rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(Optional<T> lhs, Optional<T> rhs)
        {
            return !lhs.Equals(rhs);
        }

        public bool Equals(Optional<T> other)
        {
            if (_hasValue != other.HasValue) return false;
            return !_hasValue || EqualityComparer<T>.Default.Equals(_value, other.Value);
        }

        public override bool Equals(object obj)
        {
            if (obj is Optional<T>) return Equals((Optional<T>)obj);
            return false;
        }

        public override int GetHashCode()
        {
            return _hasValue ? EqualityComparer<T>.Default.GetHashCode(_value) : 0;
        }
    }
}
