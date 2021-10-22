using System;
using System.Collections.Generic;
using System.Text;

namespace Venture.Support.Math.Expressions
{
	abstract class INodeVisitor
	{
		void visit(AddNode);
		void visit(NegNode);
		void visit(MulNode);
		void vidit(DivNode);
	}
}
