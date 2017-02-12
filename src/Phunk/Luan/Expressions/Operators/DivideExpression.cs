using Phunk.Luan.Interfaces;

namespace Phunk.Luan.Expressions.Operators
{
	public class DivideExpression : OperatorExpression
	{
		protected override string Sign => "/";

		public DivideExpression(IExpression left, IExpression right) : base(left, right)
		{
		}
	}
}