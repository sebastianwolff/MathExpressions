using System;
using System.Collections.Generic;
using System.Text;

namespace Expressionator.Expressions
{
	public class PowNode : Node
	{
		private Node _base;
		private Node _exp;

		#region prooperty accessors
		public Node powBase
		{
			get
			{
				return _base;
			}
		}
		public Node exp
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

		public override void accept(INodeVisitor visitor)
		{
			visitor.visit(this);
		}
	}
}
