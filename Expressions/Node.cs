using System;
using System.Collections.Generic;
using System.Text;

namespace Expressionator.Expressions
{
	public abstract class Node
	{
		public abstract void accept(INodeVisitor visitor);

		public override string ToString()
		{
#if false
			return ExpressionTextWriter.CreateFrom(this);
#else
			return GetType().Name;
#endif
		}
	}
}
