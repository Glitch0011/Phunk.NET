using System;
using Phunk.Luan.Interfaces;

namespace Phunk.Luan.Expressions.KeywordExpressions
{
    internal abstract class KeywordExpressionBuilder: IKeyworkBuilder
	{
		public abstract string Keyword { get; }

		public abstract IExpression Split(CodeLine raw);
		protected ExpressionBuilder Builder { get; }

        public virtual bool PreEvaluate => false;

        protected KeywordExpressionBuilder(ExpressionBuilder builder)
		{
			Builder = builder;
		}
	}
}