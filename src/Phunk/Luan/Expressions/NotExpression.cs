using Phunk.Luan.Interfaces;
using Phunk.Values;

namespace Phunk.Luan.Expressions
{
	public class NotExpression : Expression
	{
		public IExpression Value { get; set; }

		public NotExpression(IExpression value)
		{
			Value = value;
		}

        protected override Value Evaluate(ValueContainer scopes)
		{
			return Value.Process(scopes).IsTrue ? scopes.FindValue("False") : scopes.FindValue("True");
		}

		public override string ToString()
		{
			return $"!{Value}";
		}
	}
}