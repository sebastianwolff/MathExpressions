using System;
using System.Collections.Generic;
using System.Text;

namespace Expressionator.Expressions
{
	public class DateCastExpr : Node
	{
		public enum Units
		{
			Day,
			Month,
			Year,
			Hour, 
			Minute, 
			Second
		}

		private readonly Node _subExpression;
		private readonly Units _unit;

		public Node SubExpression
		{
			get
			{
				return _subExpression;
			}
		}

		public Units Unit
		{
			get
			{
				return _unit;
			}
		}

		public DateCastExpr(Node subExpression, Units unit)
		{
			_subExpression = subExpression;
			_unit = unit;
		}

		public override void Accept(INodeVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}
