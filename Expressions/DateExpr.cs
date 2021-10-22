using System;
using System.Collections.Generic;
using System.Text;

namespace Expressionator.Expressions
{
	public class DateExpr : Node
	{
		DateTime _date;

		public DateTime date
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

		public override void accept(INodeVisitor visitor)
		{
			visitor.visit(this);
		}
}
}
