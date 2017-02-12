using Phunk.Luan.Interfaces;

namespace Phunk.Luan.Expressions.Operators
{
	public class MultiplicationExpression : OperatorExpression
	{
		protected override string Sign => "*";

		public MultiplicationExpression(IExpression left, IExpression right) : base(left, right)
		{
		}
	}
}