namespace Expressionator.Expressions
{
	public interface INodeVisitor
	{
		void visit(AddNode node);
		void visit(NegNode node);
		void visit(MulNode node);
		void visit(DivNode node);
		void visit(NumberNode node);
		void visit(PowNode node);
		void visit(CondExpr node);
		void visit(CompareNode node);
		void visit(VariableNode node);
		void visit(RangeNode node);
		void visit(ContainsNode node);
		void visit(DateExpr node);
		void visit(DateCastExpr node);
		void visit(DateQualExpr node);
		void visit(TimeSpanCastExpr node);
		void visit(RoundCastExpr node);
		void visit(TextNode node);
	}
}
