using System;
using System.Collections.Generic;
using System.Text;

namespace Expressionator.Expressions
{
	public class TimeExpr : Node
	{
        readonly DateTime _time;

		public DateTime Time
		{
			get
			{
				return _time;
			}
		}

		public TimeExpr(DateTime time)
		{
			_time = time;
		}

		public override void Accept(INodeVisitor visitor)
		{
			visitor.Visit(this);
		}
}
}
