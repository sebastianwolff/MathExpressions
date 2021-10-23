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
			var comparer = new ExpressionComparer();
			return comparer.Compare(primary, secondary);
		}

		public double Compare(Node primary, Node secondary)
		{
			Node tmp = this.secondary;
			try
			{
				this.secondary = secondary;
				this.result = -1;

				primary.Accept(this);

				return result;
			}
			finally
			{
				this.secondary = tmp;
			}
		}

		#region INodeVisitor Members

		public void Visit(AddNode addNode)
		{
			if (addNode.GetType() == secondary.GetType())
				if (Compare(addNode.Left, ((AddNode)secondary).Left) == 0)
					if (Compare(addNode.Right, ((AddNode)secondary).Right) == 0)
						result = 0;
		}

		public void Visit(NegNode negNode)
		{
			if (negNode.GetType() == secondary.GetType())
				if (Compare(negNode.SubExpression, ((NegNode)secondary).SubExpression) == 0)
					result = 0;
		}

		public void Visit(MulNode mulNode)
		{
			if (mulNode.GetType() == secondary.GetType())
				if (Compare(mulNode.Left, ((MulNode)secondary).Left) == 0)
					if (Compare(mulNode.Right, ((MulNode)secondary).Right) == 0)
						result = 0;
		}

		public void Visit(DivNode divNode)
		{
			if (divNode.GetType() == secondary.GetType())
				if (Compare(divNode.Left, ((DivNode)secondary).Left) == 0)
					if (Compare(divNode.Right, ((DivNode)secondary).Right) == 0)
						result = 0;
		}

		public void Visit(NumberNode numberNode)
		{
			if (numberNode.GetType() == secondary.GetType())
				result = (numberNode.Value - ((NumberNode)secondary).Value);
		}

		public void Visit(TextNode textNode)
		{
			if (textNode.GetType() == secondary.GetType())
				result = string.Compare(textNode.Value, ((TextNode)secondary).Value, StringComparison.CurrentCultureIgnoreCase);
		}

		public void Visit(PowNode powNode)
		{
			if (powNode.GetType() == secondary.GetType())
				if (Compare(powNode.PowBase, ((PowNode)secondary).PowBase) == 0)
					if (Compare(powNode.Exp, ((PowNode)secondary).Exp) == 0)
						result = 0;
		}

		public void Visit(CondExpr condNode)
		{
			if (condNode.GetType() == secondary.GetType()
					&& Compare(condNode.Expression, ((CondExpr)secondary).Expression) == 0
					&& Compare(condNode.ThenExpr, ((CondExpr)secondary).ThenExpr) == 0
					&& Compare(condNode.ElseExpr, ((CondExpr)secondary).ElseExpr) == 0)
				result = 0;
		}

		public void Visit(CompareNode compareNode)
		{
			if (compareNode.GetType() == secondary.GetType()
					&& compareNode.Compare == ((CompareNode)secondary).Compare
					&& Compare(compareNode.Left, ((CompareNode)secondary).Left) == 0
					&& Compare(compareNode.Right, ((CompareNode)secondary).Right) == 0)
				result = 0;
		}

		public void Visit(VariableNode variableNode)
		{
			if (variableNode.GetType() == secondary.GetType()
					&& variableNode.Name == ((VariableNode)secondary).Name)
				result = 0;
		}

		public void Visit(RangeNode rangeNode)
		{
			if (rangeNode.GetType() == secondary.GetType()
					&& Compare(rangeNode.Low, ((RangeNode)rangeNode).Low) == 0
					&& Compare(rangeNode.High, ((RangeNode)rangeNode).High) == 0)
				result = 0;
		}

		public void Visit(ContainsNode contains)
		{
			if (contains.GetType() == secondary.GetType()
					&& Compare(contains.Test, ((ContainsNode)secondary).Test) == 0
					&& Compare(contains.Range, ((ContainsNode)secondary).Range) == 0)
				result = 0;
		}

		public void Visit(DateExpr date)
		{
			if (date.GetType() == secondary.GetType()
					&& date.Date == ((DateExpr)secondary).Date)
				result = (date.Date.Ticks - ((DateExpr)secondary).Date.Ticks);
		}

		public void Visit(DateCastExpr dateCast)
		{
			if (dateCast.GetType() == secondary.GetType()
					&& dateCast.Unit == ((DateCastExpr)secondary).Unit
					&& Compare(dateCast.SubExpression, ((DateCastExpr)secondary).SubExpression) == 0)
				result = 0;
		}

		public void Visit(DateQualExpr dateQual)
		{
			if (dateQual.GetType() == secondary.GetType()
					&& dateQual.Qual == ((DateQualExpr)secondary).Qual
					&& Compare(dateQual.Expression, ((DateQualExpr)secondary).Expression) == 0)
				result = 0;
		}

		public void Visit(TimeSpanCastExpr timeSpan)
		{
			if (timeSpan.GetType() == secondary.GetType()
					&& timeSpan.TimeSpan == ((TimeSpanCastExpr)secondary).TimeSpan)
				result = 0;
		}

		public void Visit(RoundCastExpr roundCast)
		{
			if (roundCast.GetType() == secondary.GetType()
					&& roundCast.Decimals == ((RoundCastExpr)secondary).Decimals
					&& Compare(roundCast.Expression, ((RoundCastExpr)secondary).Expression) == 0)
				result = 0;
		}
		#endregion
	}
}
