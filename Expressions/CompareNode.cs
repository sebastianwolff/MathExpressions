using System;
using System.Collections.Generic;
using System.Text;

namespace Expressionator.Expressions
{
	public class CompareNode : Node
	{
		public enum CompareTypes
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

        readonly Node _left;
        readonly Node _right;
        readonly CompareTypes _compareType;

		#region public attribute accessors
		public Node Left
		{
			get
			{
				return _left;
			}
		}
		public Node Right
		{
			get
			{
				return _right;
			}
		}
		public CompareTypes Compare
		{
			get
			{
				return _compareType;
			}
		}
		#endregion

		public CompareNode(CompareTypes compare, Node left, Node right)
		{
			_compareType = compare;
			_left = left;
			_right = right;
		}

		public override void Accept(INodeVisitor visitor)
		{
			visitor.Visit(this);
		}
}
}
