using Phunk.Luan.Interfaces;

namespace Phunk.Luan.Expressions.Operators
{
    internal class DivideExpression : OperatorExpression
	{
		protected override string Sign => "/";

		public DivideExpression(IExpression left, IExpression right) : base(left, right)
		{

		}
	}
}