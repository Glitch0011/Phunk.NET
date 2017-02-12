using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Phunk.Luan.Interfaces;
using Phunk.Values;

namespace Phunk.Luan.Expressions
{
	public abstract class Expression : IExpression
	{
		public virtual IList<IExpression> Children => null;

        public CodeLine CodeLine { get; set; }

        protected abstract Value Evaluate(ValueContainer scopes);

        [DebuggerStepThrough]
        public Value Process(ValueContainer scopes)
	    {
	        Debugger?.OnPreEvaluateExpression(this, scopes);

	        var res = Evaluate(scopes); 

	        return Debugger?.OnPostEvaluateExpression(this, scopes, res) ?? res;
	    }

	    public IDebugger Debugger { get; set; }

		/*public IList<ArgumentDefinition> ExtractArguments(string stringArguments, ValueContainer contexts)
		{
			var args = ArgumentParser.Parse(stringArguments.Trim().TrimStart('(').TrimEnd(')'))
				.Select(x => x.Trim())
				.ToList();

			var arguments = args
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

			return arguments;
		}*/
	}
}