using Phunk.Luan.Interfaces;

namespace Phunk.Luan.Expressions.Operators
{
	public class SubractionExpression : OperatorExpression
	{
		protected override string Sign => "-";

		public SubractionExpression(IExpression left, IExpression right) : base(left, right)
		{
		}
	}
}