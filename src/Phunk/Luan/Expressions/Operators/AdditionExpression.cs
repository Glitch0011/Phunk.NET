using Phunk.Luan.Interfaces;

namespace Phunk.Luan.Expressions.Operators
{
    internal class AdditionExpression : OperatorExpression
	{
		protected override string Sign => "+";

		public AdditionExpression(IExpression left, IExpression right) : base(left, right)
		{
			
		}
	}
}