using System;
using System.Collections.Generic;
using System.Text;

namespace Expressionator.Expressions
{
	/// <summary>
	/// this leaf-node represents a stupid variable/constant within your expression tree.
	/// </summary>
	public class VariableNode : SymbolNode
	{
        readonly bool _constant;

		/// <summary>
		/// initializes the variable leaf-node.
		/// </summary>
		/// <param name="name">the name of your variable</param>
		/// <param name="constant">when set to true, this variable acts like a constant</param>
		public VariableNode(string name, bool constant) : base(name)
		{
			_constant = constant;
		}

		public override void Accept(INodeVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}
