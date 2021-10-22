using System;
using System.Collections.Generic;
using System.Text;

namespace Expressionator.Expressions
{
	public class CondExpr : Node
	{
		private Node _condExpr;
		private Node _thenExpr;
		private Node _elseExpr;

		#region attribute accessors
		public Node condExpr
		{
			get
			{
				return _condExpr;
			}
		}
		public Node thenExpr
		{
			get
			{
				return _thenExpr;
			}
		}
		public Node elseExpr
		{
			get
			{
				return _elseExpr;
			}
		}
		#endregion

		public CondExpr(Node condExpr, Node thenExpr, Node elseExpr)
		{
			_condExpr = condExpr;
			_thenExpr = thenExpr;
			_elseExpr = elseExpr;
		}

		public override void accept(INodeVisitor visitor)
		{
			visitor.visit(this);
		}
}
}
