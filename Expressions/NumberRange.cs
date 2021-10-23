using System;

namespace Expressionator.Utils
{
	public sealed class NumberRange<T> where T : IEquatable<T>, IComparable<T>
	{
		private readonly T _min;
		private readonly T _max;

		public NumberRange(T AMin, T AMax)
		{
			_min = AMin;
			_max = AMax;
		}

		public bool Contains(T value)
		{
			return Min.CompareTo(value) <= 0 && value.CompareTo(Max) <= 0;
		}

		public T Min
		{
			get { return _min; }
		}

		public T Max
		{
			get { return _max; }
		}

		public override bool Equals(object obj)
		{
            return obj is NumberRange<T> nr && Equals(nr);
        }

		public bool Equals(NumberRange<T> ANumberRange)
		{
			return Min.Equals(ANumberRange.Min) && Max.Equals(ANumberRange.Max);
		}

		public override int GetHashCode()
		{
			return Min.GetHashCode() ^ Max.GetHashCode();
		}

		public override string ToString()
		{
			return String.Format("{0}..{1}", Min, Max);
		}
	}
}
