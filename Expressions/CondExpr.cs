using System;
using System.Collections.Generic;
using System.Text;

namespace Expressionator.Expressions
{
	public class CondExpr : Node
	{
		private readonly Node _condExpr;
		private readonly Node _thenExpr;
		private readonly Node _elseExpr;

		#region attribute accessors
		public Node Expression
		{
			get
			{
				return _condExpr;
			}
		}
		public Node ThenExpr
		{
			get
			{
				return _thenExpr;
			}
		}
		public Node ElseExpr
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

		public override void Accept(INodeVisitor visitor)
		{
			visitor.Visit(this);
		}
}
}
