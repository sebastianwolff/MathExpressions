using System;
using System.Collections.Generic;
using System.Text;

namespace Expressionator.Expressions
{
	/// <summary>
	/// represents a general number - as a leaf-node - within the expression tree.
	/// </summary>
	public class NumberNode : Node
	{
		private  double _value;

		public double Value
		{
			get
			{
				return _value;
			}
			set
			{
                _value = value;
			}
		}

		public NumberNode(double value)
		{
			_value = value;
		}

		public override void Accept(INodeVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}
