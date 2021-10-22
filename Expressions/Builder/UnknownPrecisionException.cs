using System;
using System.Collections.Generic;
using System.Text;

namespace Expressionator.Expressions.Builder
{
	public class UnknownPrecisionException : Exception
	{

		public UnknownPrecisionException()
		: base("Unknown Precision for Round error.")
		{
		}

		public UnknownPrecisionException(string message)
			: base(String.Format("Unknown Precision for Round error: {0}. The Precision followed by Keyword round must be a natural Number e.g. round2(0.752) ", message))
		{
		}

		public UnknownPrecisionException(string messag, Exception inner)
			: base(String.Format("Unknown Precision for Round error: {0}. The Precision followed by Keyword round must be a natural Number e.g. round2(0.752)", messag), inner)
		{
		}
	}
}
