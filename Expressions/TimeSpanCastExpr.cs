using System;
using System.Collections.Generic;
using System.Text;

namespace Expressionator.Expressions
{
	public class TimeSpanCastExpr : Node
	{
		public enum Units
		{
			None,
			Day,
			Month,
			Year,
			Hour,
			Minute,
			Second
		}

		private readonly Units _unit;
		private readonly Node _timeSpan;

		public Units Unit
		{
			get { return _unit; }
		}

		public Node TimeSpan
		{
			get { return _timeSpan; }
		}

		public TimeSpanCastExpr(Node timeSpan)
		{
			_unit = Units.None;
			_timeSpan = timeSpan;
		}

		public TimeSpanCastExpr(Units unit, Node timeSpan)
		{
			_unit = unit;
			_timeSpan = timeSpan;
		}

		public override void Accept(INodeVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}
