using System;
using System.Collections.Generic;
using Phunk.Luan.Interfaces;
using Phunk.Values;
using System.Linq;

namespace Phunk.Luan.Expressions
{
    internal class AssignmentExpression : Expression
	{
		protected IExpression Left { get; }
		protected IExpression Right { get; }

		public override IList<IExpression> Children => new[] { Left, Right };

		public AssignmentExpression(IExpression left, IExpression right)
		{
			Left = left;
			Right = right;
		}

		protected override Value Evaluate(ValueContainer scopes)
		{
		    scopes = scopes.Copy();
			scopes.Stack.AccessingMember = true;

			var left = Left.Process(scopes);

            scopes.Stack.AccessingMember = false;

            var right = Right?.Process(scopes);

			//We've detected it's run across a boundary. Given it's a left-hand variable, this is incorrect given it was supposed to be a new value.
			if (Left is ValueExpression && scopes.IsCrossBoundary(left))
			{
				left = scopes.Top.GetNewValue(((ValueExpression)Left).TrimmedValue);
			}

			if (right == null)
				return null;

			if (right != null && left != null && !left.IsNull && !right.IsNull && right.IsFunction && left.IsFunction)
			{
				var leftExp = left.RawValue is FunctionAbstract
					? (FunctionAbstract) left.RawValue
					: new FunctionAbstract(left.RawValue);

				var rightExp = right.RawValue is FunctionAbstract
					? (FunctionAbstract) right.RawValue
					: new FunctionAbstract(right.RawValue);

                if (leftExp.Arguments.Select(x=>x.Type).SequenceEqual(rightExp.Arguments.Select(x=>x.Type)))
                {
                    left?.SetValue(right);
                    return left;
                }

				left.SetValue(null);

				if (left[leftExp.ArgumentsAsString] == null)
					left[leftExp.ArgumentsAsString] = scopes.Engine.NewValue(leftExp);
				else
					left[leftExp.ArgumentsAsString].SetValue(leftExp);

				if (left[rightExp.ArgumentsAsString] == null)
					left[rightExp.ArgumentsAsString] = scopes.Engine.NewValue(rightExp);
				else
					left[rightExp.ArgumentsAsString].SetValue(rightExp);

				left.SetValue(null);
			}
			else
			{
				if (left == null)
					throw new Exception("You can not set null");

				if (right == null)
					return null;

				left?.Set(right);
			}

			return left;
		}

		public override string ToString()
		{
			return $"{Left} = {Right}";
		}
	}
}