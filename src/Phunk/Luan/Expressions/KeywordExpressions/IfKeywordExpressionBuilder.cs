using System.Linq;
using Phunk.Luan.Interfaces;

namespace Phunk.Luan.Expressions.KeywordExpressions
{
    internal class IfKeywordExpressionBuilder : KeywordExpressionBuilder
	{
		public override string Keyword => "if";

		public IfKeywordExpressionBuilder(ExpressionBuilder builder)
			: base(builder)
		{

		}

		public override IExpression Split(CodeLine codeLine)
		{
		    var raw = codeLine.Raw.Trim();

			var cond = ExpressionBuilder.BraceScan(raw.ToCharArray());
			var ifTrueStr = raw.ToCharArray().Skip(cond).Skip(1).Rejoin();
			var condStr = raw.ToCharArray().Skip(1).Take(cond - 1).Rejoin();

		    var condCodeLine = new CodeLine(codeLine, condStr, 1);

			if (string.IsNullOrEmpty(ifTrueStr))
			{
				var chunk = Builder.GetNewChunk();

				return new ConditionalExpression(
					Builder.Split(condCodeLine),
					chunk,
					new ValueExpression(null));
			}
			else
			{
			    var ifTrueCodeLine = new CodeLine(codeLine, ifTrueStr, cond + 1);

				//We are a single line
				return new ConditionalExpression(
					Builder.Split(condCodeLine),
					Builder.Split(ifTrueCodeLine),
					new ValueExpression(null));
			}
		}
	}
}