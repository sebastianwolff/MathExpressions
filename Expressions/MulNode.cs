using System;
using System.Collections.Generic;
using System.Text;

namespace Expressionator.Expressions
{
	public class MulNode : Node
	{
		private Node _left;
		private Node _right;

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

		public override void accept(INodeVisitor nodeVisitor)
		{
			nodeVisitor.visit(this);
		}
	}
}
