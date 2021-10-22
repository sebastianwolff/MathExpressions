using System;

namespace Expressionator.Utils
{
	public sealed class NumberRange<T> where T : IEquatable<T>, IComparable<T>
	{
		private T FMin;
		private T FMax;

		public NumberRange(T AMin, T AMax)
		{
			FMin = AMin;
			FMax = AMax;
		}

		public bool Contains(T value)
		{
			return Min.CompareTo(value) <= 0 && value.CompareTo(Max) <= 0;
		}

		public T Min
		{
			get { return FMin; }
		}

		public T Max
		{
			get { return FMax; }
		}

		public override bool Equals(object obj)
		{
			NumberRange<T> nr = obj as NumberRange<T>;
			return nr != null ? Equals(nr) : false;
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
