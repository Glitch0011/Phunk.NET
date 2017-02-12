using System;
using Phunk.Luan.Interfaces;
using Phunk.Values;

namespace Phunk.Luan.Expressions
{
	public class EqualsExpression : AssignmentExpression
	{
		public EqualsExpression(IExpression left, IExpression right) : base(left, right)
		{

		}

        protected override Value Evaluate(ValueContainer scopes)
		{
			var left = Left.Process(scopes);
			var right = Right.Process(scopes);

			//a == a (by reference)
		    if (left.Equals(right))
		        return scopes.FindValue("true");

			//null == null
	        if (left.IsNull && right.IsNull)
		        return scopes.FindValue("true");

			//null == ?
            if (left?.IsNull ?? true ^ right?.IsNull ?? true)
				return scopes.FindValue("false");

			bool res;

			//(int)a == (double)b
			if (left?.RawValue?.GetType() != right?.RawValue?.GetType())
			{
				var rightAsLeft = right?.Convert(left?.RawValue?.GetType());
				var leftAsRight = left?.Convert(right?.RawValue?.GetType());

				res = (rightAsLeft?.Equals(left?.RawValue) ?? false) || (leftAsRight?.Equals(right?.RawValue) ?? false);
			}
			else
			{
				//(int)a == (int)b
			    res = left?.RawValue?.Equals(right?.RawValue) ?? false;
			}

			return res ? scopes.FindValue("true") : scopes.FindValue("false");
		}

		public override string ToString()
		{
			return $"({Left} == {Right})";
		}
	}
}