using System;

namespace Expressionator.Utils
{
	/// <summary>
	/// DateRange ist eine Klasse zur Darstellung einer Zeitspanne zwischen zwei konkreten Zeitpunkten.
	/// </summary>
	public sealed class DateRange : IEquatable<DateRange>
	{
		private DateTime FBegin;
		private DateTime FEnd;
		private TimeSpan FTimeSpan;

		#region helper
		private static double daysInMonth(DateTime dt)
		{
			return DateTime.DaysInMonth(dt.Year, dt.Month);
		}
		#endregion

		#region Properties
		public DateTime Begin
		{
			get { return FBegin.Date; }
		}

		public DateTime End
		{
			get { return FEnd.Date; }
		}

		public TimeSpan TimeSpan
		{
			get { return FTimeSpan; }
		}

		public long Ticks
		{
			get { return FTimeSpan.Ticks; }
		}

		public double TotalDays
		{
			get { return FTimeSpan.TotalDays; }
		}

		public double TotalMonths
		{
			get
			{
				return FBegin.Year == FEnd.Year && FBegin.Month == FEnd.Month
					? (FEnd.Day - FBegin.Day + 1) / daysInMonth(FBegin)
					: (daysInMonth(FBegin) - FBegin.Day + 1) / daysInMonth(FBegin)
						+ (FBegin.Year == FEnd.Year
							? FEnd.Day / daysInMonth(FEnd) + (FEnd.Month - FBegin.Month - 1)
							: (12 - FBegin.Month)
								+ 12 * System.Math.Max((FEnd.Year - FBegin.Year - 1), 0)
								+ System.Math.Max(FEnd.Month - 1, 0)
								+ FEnd.Day / daysInMonth(FEnd));
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

			FBegin = ABegin;
			FEnd = AEnd;
			FTimeSpan = FEnd - FBegin;
		}

		public bool Contains(DateTime AValue)
		{
			return FBegin <= AValue && AValue <= FEnd;
		}

		public override bool Equals(object obj)
		{
			DateRange dr = obj as DateRange;
			return dr != null ? Equals(dr) : false;
		}

		public bool Equals(DateRange AValue)
		{
			if (AValue == null)
				throw new ArgumentNullException("AValue");

			return FBegin == AValue.Begin && FEnd == AValue.End;
		}

		public override int GetHashCode()
		{
			return FBegin.GetHashCode() ^ FEnd.GetHashCode();
		}

		public static bool Intersects(DateRange AValue)
		{
			// it can either intersect fully, partially or none.
			throw new NotImplementedException("Not yet implemented");
		}

		public override string ToString()
		{
			return String.Format("DATE({0})..DATE({1})", FBegin, FEnd);
		}
	}
}


