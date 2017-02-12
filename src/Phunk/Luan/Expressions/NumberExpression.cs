using System;
using System.Diagnostics;
using Phunk.Luan.Interfaces;
using Phunk.Values;

namespace Phunk.Luan.Expressions
{
	public class NumberExpression : Expression
	{
		private readonly object NumberValue;

		public NumberExpression(string leftHandSide)
		{
			NumberValue = leftHandSide;
		}

        public NumberExpression(double leftHandSide)
        {
            NumberValue = leftHandSide;
        }

        protected override Value Evaluate(ValueContainer scopes)
		{
            if (NumberValue is string)
            {
                var isNumber = 0;
                var isDouble = 0.0;

                if (int.TryParse((string)NumberValue, out isNumber))
                {
                    return scopes.NewValue(isNumber, "Number");
                }
                else if (double.TryParse((string)NumberValue, out isDouble))
                {
                    return scopes.NewValue(isDouble, "Number");
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
            else if (NumberValue is int)
            {
                return scopes.NewValue((double)NumberValue, "Number");
            }
            else if (NumberValue is double)
            {
                return scopes.NewValue((double)NumberValue, "Number");
            }
            else
            {
                throw new NotSupportedException();
            }
		}

		public static implicit operator int(NumberExpression exp)
		{
			return (int) exp.NumberValue;
		}

		public override string ToString()
		{
			return $"\"{NumberValue}\"";
		}
	}
}