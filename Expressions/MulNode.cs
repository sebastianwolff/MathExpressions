using System;
using System.Collections.Generic;
using System.Text;

namespace Expressionator.Expressions
{
	public class MulNode : Node
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

		public MulNode()
		{
			_left = null;
			_right = null;
		}

		public MulNode(Node left, Node right)
		{
			_left = left;
			_right = right;
		}

		~MulNode()
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
