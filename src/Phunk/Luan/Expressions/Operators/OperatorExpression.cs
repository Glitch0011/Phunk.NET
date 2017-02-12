using System;
using System.Collections.Generic;
using System.Linq;
using Phunk.Luan.Exceptions;
using Phunk.Luan.Interfaces;
using Phunk.Values;

namespace Phunk.Luan.Expressions.Operators
{
	public class OperatorExpression : AssignmentExpression
	{
		protected virtual string Sign { get; } = string.Empty;

		public override IList<IExpression> Children => new[] {Left, Right};

		public OperatorExpression(IExpression left, IExpression right, string sign = null) : base(left, right)
		{
			Sign = sign;
		}

        protected override Value Evaluate(ValueContainer scopes)
		{
			var a = Left?.Process(scopes);
			var b = Right?.Process(scopes);

			/*if ((a?.IsNull ?? true) || (b?.IsNull ?? true))
				throw new Exception("Trying to peform operator on nulll value");*/

			var operatorLambdas =
				(a?.ContainsWithTreeScan(Sign) ?? new List<Value>()).Union(b?.ContainsWithTreeScan(Sign) ?? new List<Value>())
					.Distinct().ToList();

			if (operatorLambdas.Count > 0)
			{
				var functionAbstractOperatorLambdas = operatorLambdas.Select(x => x.RawValue).OfType<FunctionAbstract>().ToList();

				var typedExpression = functionAbstractOperatorLambdas.FirstOrDefault(x =>
				{
					//List of class types available
					var argList = x.Arguments.Select(y => y.Type != null ? scopes.FindValue(y.Type) : null).ToList();

					var availableArgs = new[] {a?.Classes, b?.Classes};

					//Detect if the class types match up
					for (var i = 0; i < Math.Min(argList.Count, availableArgs.Length); i++)
						if (argList[i] != null && ((!availableArgs[i]?.Contains(argList[i])) ?? false))
							return false;

					return true;
				});

				if (typedExpression != null)
				{
					return FunctionCallExpression.InvokeSomething(scopes,
						new[] {a, b},
						typedExpression);
				}

				throw new ValidOperatorNotFoundException(functionAbstractOperatorLambdas, a?.Class?.LastName, b?.Class?.LastName, Sign);
			}

			throw new OperatorNotFoundException();
		}

		public override string ToString()
		{
			return $"({Left} {Sign} {Right})";
		}
	}
}