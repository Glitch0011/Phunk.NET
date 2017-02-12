using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Phunk.Luan.Interfaces;

namespace Phunk.Luan.Expressions
{
    internal class FunctionAbstract
	{
		public object Raw { get; set; }

		public FunctionAbstract(object raw)
		{
			Raw = raw;
		}

		public LambdaExpression Lambda => Raw as LambdaExpression;
		public Delegate AsDelegate => Raw as Delegate;

		public IList<ArgumentDefinition> Arguments 
		{
			get
			{
				if (AsDelegate != null)
				{
                    //TODO implement this
                    return null;

					//var par = AsDelegate.Method.GetParameters();
					//return par.Select(x => new ArgumentDefinition(x.ParameterType.Name, x.Name)).ToList();
				}
				else if (Lambda != null)
				{
					return Lambda.Arguments;
				}
				else
				{
					return null;
				}
			}
		}

		public string ArgumentsAsString => string.Join(", ", Arguments);

		public override string ToString()
		{
			if (Lambda != null)
				return Lambda.ToString();
			else if (AsDelegate != null)
				return AsDelegate.ToString();
			else
				return "FunctionAbstract";
		}
	}
}