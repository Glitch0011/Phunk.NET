using System;
using System.Linq;
using Phunk.Luan.Interfaces;

namespace Phunk.Luan.Expressions.KeywordExpressions
{
	internal class ForLoopExpressionBuilder : KeywordExpressionBuilder
	{
        public override string Keyword => "for";

		public ForLoopExpressionBuilder(ExpressionBuilder builder) : base(builder)
		{
			
		}

		public override IExpression Split(CodeLine codeLine)
		{
		    var raw = codeLine.Raw.Trim();

			var cond = ExpressionBuilder.BraceScan(raw.ToCharArray());

			var forArguments = raw.ToCharArray().Skip(1).Take(cond - 1).Rejoin();

			var components = forArguments.Split(';');
			
			if (components.Length == 3)
			{
				var initExpression = components.First();
				var condExpression = components.Skip(1).First();
				var incrementExpression = components.Skip(2).First();

			    var initExpressionCodeLine = new CodeLine(codeLine, initExpression, 1);

			    var condExpressionCodeLine = new CodeLine(codeLine, condExpression, 1 + initExpression.Length);

			    var incrementExpressionCodeLine = new CodeLine(codeLine, incrementExpression,
			        1 + initExpression.Length + condExpression.Length);

			    return new ForLoopExpression(Builder.Split(initExpressionCodeLine), Builder.Split(condExpressionCodeLine),
			        Builder.Split(incrementExpressionCodeLine), Builder.GetNewChunk());
			}
			else
			{
				throw new NotImplementedException();
			}
		}
	}
}
