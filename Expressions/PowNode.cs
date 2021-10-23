using System;
using System.Collections.Generic;
using System.Text;

namespace Expressionator.Expressions
{
	public class PowNode : Node
	{
		private readonly Node _base;
		private readonly Node _exp;

		#region prooperty accessors
		public Node PowBase
		{
			get
			{
				return _base;
			}
		}
		public Node Exp
		{
			get
			{
				return _exp;
			}
		}
		#endregion

		public PowNode(Node powBase, Node exp)
		{
			_base = powBase;
			_exp = exp;
		}

		public override void Accept(INodeVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}
