using System;
using System.Collections.Generic;
using System.Text;

namespace Expressionator.Expressions
{
	public class DivNode : Node
	{
		private Node _left;
		private Node _right;

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

		public DivNode()
		{
			_left = null;
			_right = null;
		}

		public DivNode(Node left, Node right)
		{
			_left = left;
			_right = right;
		}

		~DivNode()
		{
			_left = null;
			_right = null;
		}

		public override void Accept(INodeVisitor nodeVisitor)
		{
			nodeVisitor.Visit(this);
		}
	}
}
