using Phunk.Luan.Interfaces;
using Phunk.Values;

namespace Phunk.Luan.Expressions
{
    internal class StringExpression : Expression
	{
		private readonly string StringValue;

		public StringExpression(string leftHandSide)
		{
			StringValue = leftHandSide;
		}

        protected override Value Evaluate(ValueContainer scopes)
		{
			return scopes.NewValue(StringValue, "String");
		}

		public static implicit operator string(StringExpression exp)
		{
			return exp.StringValue;
		}

		public override string ToString()
		{
			return $"\"{StringValue}\"";
		}
	}
}