using System;
using System.Collections.Generic;
using System.Text;

namespace Expressionator.Expressions
{
	public class RoundCastExpr : Node
	{
		private int _decimals;
		private Node _expression;

		public int decimals
		{
			get
			{
				return _decimals;
			}
		}

		public Node expression
		{
			get
			{
				return _expression;
			}
		}

		public RoundCastExpr(int decimals, Node expression)
		{
			_decimals = decimals;
			_expression = expression;
		}

		public override void accept(INodeVisitor visitor)
		{
			visitor.visit(this);
		}
	}
}
