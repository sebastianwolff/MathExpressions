using System;
using System.Collections.Generic;
using System.Text;

namespace Expressionator.Expressions.Evaluator
{
	public class ExpressionEvaluationException : Exception
	{
		public ExpressionEvaluationException()
			: base("Expression evaluation error.")
		{
		}

		public ExpressionEvaluationException(string message)
			: base(String.Format("Expression evaluation error: {0}", message))
		{
		}

		public ExpressionEvaluationException(string messag, Exception inner)
			: base(String.Format("Expression evaluation error: {0}", messag), inner)
		{
		}
	}
}
