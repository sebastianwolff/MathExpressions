using System;
using System.Collections.Generic;
using System.Text;

namespace Expressionator.Expressions {
	/// <summary>
	/// represents a general number - as a leaf-node - within the expression tree.
	/// </summary>
	public class TextNode : Node {
		private string FValue;

		public string Value {
			get { return FValue; }
			set { FValue = value; }
		}

		public TextNode(string AValue) {
			FValue = AValue;
		}

		public override void Accept(INodeVisitor visitor) {
			visitor.Visit(this);
		}
	}
}
