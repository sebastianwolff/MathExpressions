using System;
using System.Collections.Generic;
using System.Text;

namespace Expressionator.Expressions
{
	public class AddNode : Node
	{
		private Node _left;
		private Node _right;

		#region property accessors
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
		#endregion

		public AddNode() 
		{
			_left = null;
			_right = null;
		}

		public AddNode(Node left, Node right)
		{
			_left = left;
			_right = right;
		}

		~AddNode()
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
