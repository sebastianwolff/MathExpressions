using System;
using System.Collections.Generic;
using System.Text;

namespace Expressionator.Expressions
{
	public class ContainsNode : Node
	{
		private readonly Node _test;
		private readonly RangeNode _range;

		#region property accessors
		public Node Test
		{
			get
			{
				return _test;
			}
		}

		public RangeNode Range
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

		public override void Accept(INodeVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}
