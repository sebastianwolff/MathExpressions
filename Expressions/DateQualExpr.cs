using System;
using System.Collections.Generic;
using System.Text;

namespace Expressionator.Expressions
{
	public class DateQualExpr : Node
	{
		public enum Quals
		{
			Year,
			Month,
			Day
		}

		private readonly Quals _qual;
		private readonly Node _expression;

		public Quals Qual
		{
			get
			{
				return _qual;
			}
		}

		public Node Expression
		{
			get
			{
				return _expression;
			}
		}

		public DateQualExpr(Quals qual, Node expression)
		{
			_qual = qual;
			_expression = expression;
		}

		public override void Accept(INodeVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}
