using System;
using System.Collections.Generic;
using System.Text;

namespace Expressionator.Expressions
{
	interface IExpressionBuilder
	{
		Node Parse(string expressionPattern);
	}
}
