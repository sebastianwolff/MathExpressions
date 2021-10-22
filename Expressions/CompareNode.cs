using System;
using System.Collections.Generic;
using System.Text;

namespace Expressionator.Expressions
{
	public class CompareNode : Node
	{
		public enum Compare
		{
			Less,
			LessEqual,
			Equal,
			UnEqual,
			GreaterEqual,
			Greater,

			And,
			Or,
			XOr
		}

		Node _left;
		Node _right;
		Compare _compare;

		#region public attribute accessors
		public Node left
		{
			get
			{
				return _left;
			}
		}
		public Node right
		{
			get
			{
				return _right;
			}
		}
		public Compare compare
		{
			get
			{
				return _compare;
			}
		}
		#endregion

		public CompareNode(Compare compare, Node left, Node right)
		{
			_compare = compare;
			_left = left;
			_right = right;
		}

		public override void accept(INodeVisitor visitor)
		{
			visitor.visit(this);
		}
}
}
