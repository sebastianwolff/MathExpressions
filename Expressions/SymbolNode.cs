using System;
using System.Collections.Generic;
using System.Text;

namespace Expressionator.Expressions
{
	public abstract class SymbolNode : Node
	{
		private string _name;

		public string name
		{
			get
			{
				return _name;
			}
		}

		public SymbolNode(string name)
		{
			_name = name;
		}
	}
}
