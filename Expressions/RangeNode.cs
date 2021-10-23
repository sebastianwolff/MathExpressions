using System;
using System.Collections.Generic;
using System.Text;

namespace Expressionator.Expressions
{
	public class RangeNode : Node
	{
		private readonly Node _low;
		private readonly Node _high;

		#region property accessors
		public Node Low
		{
			get
			{
				return _low;
			}
		}

		public Node High
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

		public override void Accept(INodeVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}
