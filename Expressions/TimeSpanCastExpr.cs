using System;
using System.Collections.Generic;
using System.Text;

namespace Expressionator.Expressions
{
	public class TimeSpanCastExpr : Node
	{
		public enum Unit
		{
			None,
			Day,
			Month,
			Year
		}

		private Unit _unit;
		private Node _timeSpan;

		public Unit unit
		{
			get { return _unit; }
		}

		public Node timeSpan
		{
			get { return _timeSpan; }
		}

		public TimeSpanCastExpr(Node timeSpan)
		{
			_unit = Unit.None;
			_timeSpan = timeSpan;
		}

		public TimeSpanCastExpr(Unit unit, Node timeSpan)
		{
			_unit = unit;
			_timeSpan = timeSpan;
		}

		public override void accept(INodeVisitor visitor)
		{
			visitor.visit(this);
		}
	}
}
