using System.Collections.Generic;
using Phunk.Luan.Interfaces;
using Phunk.Values;

namespace Phunk.Luan.Expressions
{
	public class BracketsExpression : Expression
	{
		public IExpression SubExpression { get; set; }

		public override IList<IExpression> Children => new[] { SubExpression };

		public BracketsExpression(IExpression subExpression)
		{
			SubExpression = subExpression;
		}

		public override string ToString()
		{
			return $"({SubExpression})";
		}

        protected override Value Evaluate(ValueContainer scopes)
		{
			return SubExpression.Process(scopes);
		}
	}
}