using System;
using System.Collections.Generic;
using System.Text;

namespace Expressionator.Expressions
{
	public class RoundCastExpr : Node
	{
		private readonly int _decimals;
		private readonly Node _expression;

		public int Decimals
		{
			get
			{
				return _decimals;
			}
		}

		public Node Expression
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

		public override void Accept(INodeVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}
