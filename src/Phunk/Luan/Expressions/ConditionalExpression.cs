using Phunk.Luan.Interfaces;
using Phunk.Values;

namespace Phunk.Luan.Expressions
{
	public class ConditionalExpression : Expression
	{
		private IExpression Conditional { get; set; }
		private IExpression IfTrue { get; set; }
		private IExpression IfFalse { get; set; }

		public ConditionalExpression(IExpression conditional, IExpression iftrue, IExpression ifFalse)
		{
			Conditional = conditional;
			IfTrue = iftrue;
			IfFalse = ifFalse;
		}

        protected override Value Evaluate(ValueContainer scopes)
		{
			var cond = Conditional.Process(scopes);
			
			return !cond.IsNull && cond ? IfTrue?.Process(scopes) : IfFalse?.Process(scopes);
		}

		public override string ToString()
		{
			return $"{Conditional} ? {IfTrue} : {IfFalse}";
		}
	}
}