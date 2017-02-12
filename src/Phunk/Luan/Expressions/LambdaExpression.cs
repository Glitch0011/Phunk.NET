using System.Collections.Generic;
using System.Linq;
using Phunk.Luan.Interfaces;
using Phunk.Values;
using System.Text;
using System;

namespace Phunk.Luan.Expressions
{
    internal class LambdaExpression : Expression
	{
		public IList<ArgumentDefinition> Arguments { get; }

		public IExpression RightHandSide { get; }

		public override IList<IExpression> Children => new[] { RightHandSide };

		public string ArgumentsAsString => string.Join(", ", Arguments);

		public Executable Executor { get; set; }

		public LambdaExpression(string stringArguments, IExpression rightHandSide, Executable ownerExecutor)
		{
			Executor = ownerExecutor;

			var args = ArgumentParser.Parse(stringArguments.Trim().TrimStart('(').TrimEnd(')'))
				.Select(x => x.value.Trim()).ToList();

			Arguments = args
				.Select(x => x.Split(' '))
				.Select(x =>
				{
					if (x.Count() == 1)
					{
						return new ArgumentDefinition(null, x[0]);
					}
					if (x.Count() == 2)
					{
						return new ArgumentDefinition(x[0], x[1]);
					}
					return null;
				})
				.ToList();

			RightHandSide = rightHandSide;
		}

		public override string ToString()
		{
            if (RightHandSide is ChunkExpression)
            {
                var chunk = (ChunkExpression)RightHandSide;

                /*if (chunk.Expressions.Count == 1)
                {
                    return $"({ArgumentsAsString}) => {RightHandSide}";
                }
                else
                {*/
                    var str = new StringBuilder();

                    str.AppendLine($"({ArgumentsAsString}) =>");

                    foreach (var line in RightHandSide.ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.None))
                        str.AppendLine($"\t{line}");

                    return str.ToString();
                //}
            }
            else
            {
                return $"({ArgumentsAsString}) => {RightHandSide}";
            }
		}	

        protected override Value Evaluate(ValueContainer scopes)
		{
			return scopes.NewValue(new FunctionAbstract(this), scopes.FindValue("Expression"));
		}
	}
}