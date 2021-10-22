using System;
using System.Collections.Generic;
using System.Text;

namespace Expressionator.Expressions
{
	/// <summary>
	/// You wanna use this class's API when you have to compare two expression trees.
	/// </summary>
	public class ExpressionComparer : INodeVisitor
	{
		private Node secondary;
		private double result;

		public static double CompareExpression(Node primary, Node secondary)
		{
			ExpressionComparer comparer = new ExpressionComparer();
			return comparer.Compare(primary, secondary);
		}

		public double Compare(Node primary, Node secondary)
		{
			Node tmp = this.secondary;
			try
			{
				this.secondary = secondary;
				this.result = -1;

				primary.accept(this);

				return result;
			}
			finally
			{
				this.secondary = tmp;
			}
		}

		#region INodeVisitor Members

		public void visit(AddNode addNode)
		{
			if (addNode.GetType() == secondary.GetType())
				if (Compare(addNode.left, ((AddNode)secondary).left) == 0)
					if (Compare(addNode.right, ((AddNode)secondary).right) == 0)
						result = 0;
		}

		public void visit(NegNode negNode)
		{
			if (negNode.GetType() == secondary.GetType())
				if (Compare(negNode.subExpression, ((NegNode)secondary).subExpression) == 0)
					result = 0;
		}

		public void visit(MulNode mulNode)
		{
			if (mulNode.GetType() == secondary.GetType())
				if (Compare(mulNode.left, ((MulNode)secondary).left) == 0)
					if (Compare(mulNode.right, ((MulNode)secondary).right) == 0)
						result = 0;
		}

		public void visit(DivNode divNode)
		{
			if (divNode.GetType() == secondary.GetType())
				if (Compare(divNode.left, ((DivNode)secondary).left) == 0)
					if (Compare(divNode.right, ((DivNode)secondary).right) == 0)
						result = 0;
		}

		public void visit(NumberNode numberNode)
		{
			if (numberNode.GetType() == secondary.GetType())
				result = (numberNode.value - ((NumberNode)secondary).value);
		}

		public void visit(TextNode textNode)
		{
			if (textNode.GetType() == secondary.GetType())
				result = string.Compare(textNode.Value, ((TextNode)secondary).Value, StringComparison.CurrentCultureIgnoreCase);
		}

		public void visit(PowNode powNode)
		{
			if (powNode.GetType() == secondary.GetType())
				if (Compare(powNode.powBase, ((PowNode)secondary).powBase) == 0)
					if (Compare(powNode.exp, ((PowNode)secondary).exp) == 0)
						result = 0;
		}

		public void visit(CondExpr condNode)
		{
			if (condNode.GetType() == secondary.GetType()
					&& Compare(condNode.condExpr, ((CondExpr)secondary).condExpr) == 0
					&& Compare(condNode.thenExpr, ((CondExpr)secondary).thenExpr) == 0
					&& Compare(condNode.elseExpr, ((CondExpr)secondary).elseExpr) == 0)
				result = 0;
		}

		public void visit(CompareNode compareNode)
		{
			if (compareNode.GetType() == secondary.GetType()
					&& compareNode.compare == ((CompareNode)secondary).compare
					&& Compare(compareNode.left, ((CompareNode)secondary).left) == 0
					&& Compare(compareNode.right, ((CompareNode)secondary).right) == 0)
				result = 0;
		}

		public void visit(VariableNode variableNode)
		{
			if (variableNode.GetType() == secondary.GetType()
					&& variableNode.name == ((VariableNode)secondary).name)
				result = 0;
		}

		public void visit(RangeNode rangeNode)
		{
			if (rangeNode.GetType() == secondary.GetType()
					&& Compare(rangeNode.low, ((RangeNode)rangeNode).low) == 0
					&& Compare(rangeNode.high, ((RangeNode)rangeNode).high) == 0)
				result = 0;
		}

		public void visit(ContainsNode contains)
		{
			if (contains.GetType() == secondary.GetType()
					&& Compare(contains.test, ((ContainsNode)secondary).test) == 0
					&& Compare(contains.range, ((ContainsNode)secondary).range) == 0)
				result = 0;
		}

		public void visit(DateExpr date)
		{
			if (date.GetType() == secondary.GetType()
					&& date.date == ((DateExpr)secondary).date)
				result = (date.date.Ticks - ((DateExpr)secondary).date.Ticks);
		}

		public void visit(DateCastExpr dateCast)
		{
			if (dateCast.GetType() == secondary.GetType()
					&& dateCast.unit == ((DateCastExpr)secondary).unit
					&& Compare(dateCast.subExpression, ((DateCastExpr)secondary).subExpression) == 0)
				result = 0;
		}

		public void visit(DateQualExpr dateQual)
		{
			if (dateQual.GetType() == secondary.GetType()
					&& dateQual.qual == ((DateQualExpr)secondary).qual
					&& Compare(dateQual.expression, ((DateQualExpr)secondary).expression) == 0)
				result = 0;
		}

		public void visit(TimeSpanCastExpr timeSpan)
		{
			if (timeSpan.GetType() == secondary.GetType()
					&& timeSpan.timeSpan == ((TimeSpanCastExpr)secondary).timeSpan)
				result = 0;
		}

		public void visit(RoundCastExpr roundCast)
		{
			if (roundCast.GetType() == secondary.GetType()
					&& roundCast.decimals == ((RoundCastExpr)secondary).decimals
					&& Compare(roundCast.expression, ((RoundCastExpr)secondary).expression) == 0)
				result = 0;
		}
		#endregion
	}
}
