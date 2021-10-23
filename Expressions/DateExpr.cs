using System;
using System.Collections.Generic;
using System.Text;

namespace Expressionator.Expressions
{
	public class DateExpr : Node
	{
        readonly DateTime _date;

		public DateTime Date
		{
			get
			{
				return _date.Date;
			}
		}

		public DateExpr(DateTime date)
		{
			_date = date.Date;
		}

		public override void Accept(INodeVisitor visitor)
		{
			visitor.Visit(this);
		}
}
}
