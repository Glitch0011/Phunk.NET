using Phunk.Luan.Interfaces;
using Phunk.Values;

namespace Phunk.Luan.Expressions
{
	public class ForLoopExpression : Expression
	{
		public IExpression OnExecute { get; set; }
		private IExpression InitExpression { get; }
		private IExpression CondExpression { get; }
		private IExpression IncrementExpression { get; }

		public ForLoopExpression(IExpression initExpression, IExpression condExpression, IExpression incrementExpression,
			IExpression onExecute)
		{
			OnExecute = onExecute;
			InitExpression = initExpression;
			CondExpression = condExpression;
			IncrementExpression = incrementExpression;
		}

        protected override Value Evaluate(ValueContainer scopes)
		{
			InitExpression.Process(scopes);

			Value lastVal = null;

			while (CondExpression.Process(scopes).IsTrue)
			{
                lastVal = OnExecute.Process(scopes);

				if (lastVal?.IsReturning ?? false)
					return lastVal;

				IncrementExpression.Process(scopes);
			}

			return lastVal;
		}

		public override string ToString()
		{
			return $"for ({InitExpression}; {CondExpression}; {IncrementExpression}) {OnExecute}";
		}
	}
}