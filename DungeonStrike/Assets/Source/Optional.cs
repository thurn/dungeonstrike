using System;
using System.Collections.Generic;

namespace DungeonStrike
{
    public struct Optional<T> : IEquatable<Optional<T>>
    {
        private T _value;
        private readonly bool _hasValue;
        private static Optional<T> _empty = new Optional<T>();

        public static Optional<T> Of(T value)
        {
            return new Optional<T>(value);
        }

        public static Optional<T> Empty
        {
            get { return Optional<T>._empty; }
        }

        public bool HasValue
        {
            get { return this._hasValue; }
        }

        public T Value
        {
            get
            {
                if (!this.HasValue) throw new InvalidOperationException("Optional does not have a value.");
                return this._value;
            }
        }

        internal Optional(T value)
        {
            this._hasValue = true;
            this._value = value;
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
            if (this.HasValue != other.HasValue) return false;
            if (!this.HasValue) return true;
            return EqualityComparer<T>.Default.Equals(this._value, other.Value);
        }

        public override bool Equals(object other)
        {
            if (other is Optional<T>) return Equals((Optional<T>)other);
            return false;
        }

        public override int GetHashCode()
        {
            if (!this._hasValue) return 0;
            return EqualityComparer<T>.Default.GetHashCode(this._value);
        }
    }
}
