using Phunk.Luan.Interfaces;

namespace Phunk.Luan.Expressions.Operators
{
	public class AdditionExpression : OperatorExpression
	{
		protected override string Sign => "+";

		public AdditionExpression(IExpression left, IExpression right) : base(left, right)
		{
			
		}
	}
}