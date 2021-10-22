using System;
using System.Collections.Generic;
using System.Text;

namespace Expressionator.Expressions
{
	public class NegNode : Node
	{
		private Node _subExpression;

		public Node subExpression
		{
			get
			{
				return _subExpression;
			}
		}

		public NegNode(Node expression)
		{
			_subExpression = expression;
		}

		~NegNode()
		{
			_subExpression = null;
		}

		public override void accept(INodeVisitor visitor)
		{
			visitor.visit(this);
		}
	}
}
