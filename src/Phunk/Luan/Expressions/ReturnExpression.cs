using Phunk.Luan.Interfaces;
using Phunk.Values;

namespace Phunk.Luan.Expressions
{
	public class ReturnExpression : Expression
	{
		private readonly IExpression returnValue;

		public ReturnExpression(IExpression expression)
		{
			returnValue = expression;
		}

        protected override Value Evaluate(ValueContainer scopes)
		{
			return returnValue.Process(scopes);
		}

		public override string ToString()
		{
			return $"return {returnValue}";
		}
	}
}