using System.Collections.Generic;
using Phunk.Luan.Interfaces;
using Phunk.Values;
using System.Diagnostics;
using System;

namespace Phunk.Luan.Expressions
{
	public class ChunkExpression : Expression
	{
		public IList<IExpression> Expressions { get; } = new List<IExpression>();

        protected override Value Evaluate(ValueContainer scopes)
		{
			Value lastVal = null;

			foreach (var chunk in Expressions)
			{
				lastVal = RunChunk(scopes, chunk);

				//Support for nested chunks in a function
				if (lastVal?.IsReturning ?? false)
				{
					lastVal.IsReturning = false;
					return lastVal;
				}

				if (chunk is ReturnExpression)
				{
					lastVal.IsReturning = true;
					return lastVal;
				}
			}

			return lastVal;
		}

        [DebuggerStepThrough]
        private static Value RunChunk(ValueContainer scopes, IExpression chunk)
	    {
	        return chunk?.Process(scopes);
	    }

	    public override IList<IExpression> Children => Expressions;

		public override string ToString()
		{
            return string.Join(Environment.NewLine, Expressions).Trim();
		}

		public void Add(IExpression splitBody)
		{
			Expressions.Add(splitBody);
		}
	}
}