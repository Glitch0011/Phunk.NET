using System;
using Phunk.Luan.Interfaces;
using Phunk.Values;

namespace Phunk.Luan.Expressions
{
	public class CommentExpression : Expression
	{
		public string text;

		public CommentExpression(string text)
		{
			this.text = text;
		}

        protected override Value Evaluate(ValueContainer scopes)
		{
		    Console.WriteLine("Comment: " + text);

			return null;
		}

		public override string ToString()
		{
			return "// " + text;
		}
	}
}