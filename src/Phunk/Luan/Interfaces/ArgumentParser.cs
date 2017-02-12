using System;
using System.Collections.Generic;
using System.Linq;

namespace Phunk.Luan.Interfaces
{
	public class ArgumentResult
	{
		public string value;
		public int position;
	}

	public static class ArgumentParser
	{
		public static IList<ArgumentResult> Parse(string stringArguments)
		{
			var set = stringArguments.ToCharArray().ToList();

			var openBrace = 0;
			var openQuote = 0;

			var argument = string.Empty;
			var arguments = new List<ArgumentResult>();

			int i = 0;
			foreach (var t in set)
			{
				if (t == '"')
				{
					if (openQuote == 1)
					{
						openQuote--;
					}
					else
					{
						openQuote++;
					}
				}
				if (t == '(')
				{
					openBrace++;
				}
				else if (t == ')')
				{
					openBrace--;
				}
				else if (t == ',' && openQuote == 0)
				{
					if (openBrace == 0)
					{
						arguments.Add(new ArgumentResult() {position = i, value = argument});
						argument = string.Empty;
						continue;
					}
				}

				argument += t;
				i++;
			}
			if (openBrace == 0 && !string.IsNullOrEmpty(argument))
			{
				arguments.Add(new ArgumentResult() {position = i, value = argument});
			}
			else
			{
				if (!string.IsNullOrEmpty(argument))
				{
					throw new Exception("ASDF");
				}
			}

			return arguments;
		}
	}
}