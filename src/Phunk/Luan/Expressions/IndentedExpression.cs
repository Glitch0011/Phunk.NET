using System;
using Phunk.Luan.Interfaces;
using Phunk.Values;

namespace Phunk.Luan.Expressions
{
    internal class IndentedExpression : Expression
	{
		public IExpression Expression { get; }

		public IndentedExpression(IExpression expression)
		{
			Expression = expression;
		}

		public override string ToString()
		{
			return $"    {Expression}";
		}

        protected override Value Evaluate(ValueContainer scopes)
		{
			throw new NotImplementedException();
		}
	}
}