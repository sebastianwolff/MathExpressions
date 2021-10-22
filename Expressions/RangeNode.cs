using System;
using System.Collections.Generic;
using System.Text;

namespace Expressionator.Expressions
{
	public class RangeNode : Node
	{
		private Node _low;
		private Node _high;

		#region property accessors
		public Node low
		{
			get
			{
				return _low;
			}
		}

		public Node high
		{
			get
			{
				return _high;
			}
		}
		#endregion

		public RangeNode(Node low, Node high)
		{
			_low = low;
			_high = high;
		}

		public override void accept(INodeVisitor visitor)
		{
			visitor.visit(this);
		}
	}
}
