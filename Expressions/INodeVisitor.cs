namespace Expressionator.Expressions
{
	public interface INodeVisitor
	{
		void Visit(AddNode node);
		void Visit(NegNode node);
		void Visit(MulNode node);
		void Visit(DivNode node);
		void Visit(NumberNode node);
		void Visit(PowNode node);
		void Visit(CondExpr node);
		void Visit(CompareNode node);
		void Visit(VariableNode node);
		void Visit(RangeNode node);
		void Visit(ContainsNode node);
		void Visit(DateExpr node);
		void Visit(TimeExpr node);
		void Visit(DateCastExpr node);
		void Visit(DateQualExpr node);
		void Visit(TimeSpanCastExpr node);
		void Visit(RoundCastExpr node);
		void Visit(TextNode node);
	}
}
