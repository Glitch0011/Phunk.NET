using System;
using Phunk.Luan.Interfaces;
using Phunk.Values;

namespace Phunk.Luan.Expressions
{
    internal class NotEqualsExpression : AssignmentExpression
	{
		public NotEqualsExpression(IExpression left, IExpression right) : base(left, right)
		{

		}

        protected override Value Evaluate(ValueContainer scopes)
		{
			var left = Left.Process(scopes);
			var right = Right.Process(scopes);

			if (left?.IsNull ?? true ^ right?.IsNull ?? true)
				return scopes.FindValue("false");

			return !left.RawValue.Equals(right.RawValue) ? scopes.FindValue("true") : scopes.FindValue("false");
		}

		public override string ToString()
		{
			return $"({Left} != {Right})";
		}
	}
}