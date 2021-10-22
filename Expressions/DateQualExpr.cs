using System;
using System.Collections.Generic;
using System.Text;

namespace Expressionator.Expressions
{
	public class DateQualExpr : Node
	{
		public enum Qual
		{
			Year,
			Month,
			Day
		}

		private Qual _qual;
		private Node _expression;

		public Qual qual
		{
			get
			{
				return _qual;
			}
		}

		public Node expression
		{
			get
			{
				return _expression;
			}
		}

		public DateQualExpr(Qual qual, Node expression)
		{
			_qual = qual;
			_expression = expression;
		}

		public override void accept(INodeVisitor visitor)
		{
			visitor.visit(this);
		}
	}
}
