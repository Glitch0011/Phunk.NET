using Phunk.Luan.Interfaces;

namespace Phunk.Luan.Expressions.KeywordExpressions
{
    internal class ReturnKeywordExpressionBuilder : KeywordExpressionBuilder
	{
		public override string Keyword => "return";

		public ReturnKeywordExpressionBuilder(ExpressionBuilder builder)
			: base(builder)
		{

		}

		public override IExpression Split(CodeLine raw)
		{
			return new ReturnExpression(Builder.Split(raw));
		}
	}
}