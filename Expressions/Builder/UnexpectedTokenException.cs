using System;
using System.Collections.Generic;
using System.Text;

namespace Expressionator.Expressions.Builder
{
	public class UnexpectedTokenException : Exception
	{
		public readonly string token;
		public readonly int line;
		public readonly int column;

		public UnexpectedTokenException(string token, int line, int column)
			: base(String.Format("Unexpected token '{0}' at line {1} column {2}", token, line, column))
		{
			this.token = token;
			this.line = line;
			this.column = column;
		}
	}
}
