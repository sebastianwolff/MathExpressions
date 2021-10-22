using System;
using System.Collections.Generic;
using System.Text;

namespace Expressionator.Expressions
{
	public class ContainsNode : Node
	{
		private Node _test;
		private RangeNode _range;

		#region property accessors
		public Node test
		{
			get
			{
				return _test;
			}
		}

		public RangeNode range
		{
			get
			{
				return _range;
			}
		}
		#endregion

		public ContainsNode(Node test, RangeNode range)
		{
			_test = test;
			_range = range;
		}

		public override void accept(INodeVisitor visitor)
		{
			visitor.visit(this);
		}
	}
}
