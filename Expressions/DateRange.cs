using System;

namespace Expressionator.Utils
{
	/// <summary>
	/// DateRange ist eine Klasse zur Darstellung einer Zeitspanne zwischen zwei konkreten Zeitpunkten.
	/// </summary>
	public sealed class DateRange : IEquatable<DateRange>
	{
		private readonly DateTime _begin;
		private readonly DateTime _end;
		private readonly TimeSpan _timeSpan;

		#region helper
		private static double DaysInMonth(DateTime dt)
		{
			return DateTime.DaysInMonth(dt.Year, dt.Month);
		}
		#endregion

		#region Properties
		public DateTime Begin
		{
			get { return _begin; }
		}

		public DateTime End
		{
			get { return _end; }
		}

		public TimeSpan TimeSpan
		{
			get { return _timeSpan; }
		}

		public long Ticks
		{
			get { return _timeSpan.Ticks; }
		}

		public double TotalDays
		{
			get { return _timeSpan.TotalDays; }
		}

		public double TotalMonths
		{
			get
			{
				return _begin.Year == _end.Year && _begin.Month == _end.Month
					? (_end.Day - _begin.Day + 1) / DaysInMonth(_begin)
					: (DaysInMonth(_begin) - _begin.Day + 1) / DaysInMonth(_begin)
						+ (_begin.Year == _end.Year
							? _end.Day / DaysInMonth(_end) + (_end.Month - _begin.Month - 1)
							: (12 - _begin.Month)
								+ 12 * System.Math.Max((_end.Year - _begin.Year - 1), 0)
								+ System.Math.Max(_end.Month - 1, 0)
								+ _end.Day / DaysInMonth(_end));
			}
		}

		public double TotalYears
		{
			get { return TotalMonths / 12; }
		}
		#endregion

		public DateRange(DateTime ABegin, DateTime AEnd)
		{
			if (ABegin > AEnd)
				throw new ArgumentException("DateSpan's begin must be BEFORE the end date.");

			_begin = ABegin;
			_end = AEnd;
			_timeSpan = _end - _begin;
		}

		public bool Contains(DateTime AValue)
		{
			return _begin <= AValue && AValue <= _end;
		}

		public override bool Equals(object obj)
		{
            return obj is DateRange dr && Equals(dr);
        }

		public bool Equals(DateRange AValue)
		{
			if (AValue == null)
				throw new ArgumentNullException(nameof(AValue));

			return _begin == AValue.Begin && _end == AValue.End;
		}

		public override int GetHashCode()
		{
			return _begin.GetHashCode() ^ _end.GetHashCode();
		}

		public static bool Intersects(DateRange AValue)
		{
			// it can either intersect fully, partially or none.
			throw new NotImplementedException("Not yet implemented");
		}

		public override string ToString()
		{
			return String.Format("DATE({0})..DATE({1})", _begin, _end);
		}
	}
}


