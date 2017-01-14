using System;
using System.Collections.Generic;
using System.Text;

namespace DungeonStrike.Source.Utilities
{
    /// <summary>
    /// Helper class for creating tuples.
    /// </summary>
    /// <remarks>
    /// The API of this class should be a strict subset of the System.Tuple API introduced in .NET Framework version 4.
    /// See https://msdn.microsoft.com/en-us/library/system.tuple
    /// </remarks>
    public static class Tuple
    {
        public static Tuple<T1, T2> Create<T1, T2>(T1 item1, T2 item2)
        {
            return new Tuple<T1, T2>(item1, item2);
        }

        internal static int CombineHashCodes(int h1, int h2)
        {
            return ((h1 << 5) + h1) ^ h2;
        }
    }

    /// <summary>
    /// Class for representing tuple objects.
    /// </summary>
    /// <remarks>
    /// The API of this class should be a strict subset of the System.Tuple API introduced in .NET Framework version 4.
    /// See https://msdn.microsoft.com/en-us/library/system.tuple
    /// </remarks>
    [Serializable]
    public class Tuple<T1, T2>
    {
        private readonly T1 _item1;
        private readonly T2 _item2;

        public T1 Item1
        {
            get { return _item1; }
        }

        public T2 Item2
        {
            get { return _item2; }
        }

        public Tuple(T1 item1, T2 item2)
        {
            _item1 = item1;
            _item2 = item2;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Tuple<T1, T2>);
        }

        public bool Equals(Tuple<T1, T2> objTuple)
        {
            var comparer = EqualityComparer<object>.Default;

            if (objTuple == null)
            {
                return false;
            }

            return comparer.Equals(_item1, objTuple._item1) && comparer.Equals(_item2, objTuple._item2);
        }

        public override int GetHashCode()
        {
            var comparer = EqualityComparer<object>.Default;
            return Tuple.CombineHashCodes(comparer.GetHashCode(_item1), comparer.GetHashCode(_item2));
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("(");
            sb.Append(_item1);
            sb.Append(", ");
            sb.Append(_item2);
            sb.Append(")");
            return sb.ToString();
        }
    }
}