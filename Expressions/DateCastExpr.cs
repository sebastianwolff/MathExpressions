using System;
using System.Collections.Generic;
using System.Text;

namespace Expressionator.Expressions
{
	public class DateCastExpr : Node
	{
		public enum Unit
		{
			Day,
			Month,
			Year
		}

		private Node _subExpression;
		private Unit _unit;

		public Node subExpression
		{
			get
			{
				return _subExpression;
			}
		}

		public Unit unit
		{
			get
			{
				return _unit;
			}
		}

		public DateCastExpr(Node subExpression, Unit unit)
		{
			_subExpression = subExpression;
			_unit = unit;
		}

		public override void accept(INodeVisitor visitor)
		{
			visitor.visit(this);
		}
	}
}
