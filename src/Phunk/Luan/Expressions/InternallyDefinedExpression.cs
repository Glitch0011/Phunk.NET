using System;
using System.Collections.Generic;
using Phunk.Luan.Interfaces;
using Phunk.Values;
using System.Linq;

namespace Phunk.Luan.Expressions
{
    internal class InternallyDefinedExpression : IExpression
	{
		public Engine Engine { get; set; }
		public dynamic Value { get; set; }

		public InternallyDefinedExpression(Engine engine, Value value)
		{
			Engine = engine;
			Value = value;
		}

        protected Value Evaluate(ValueContainer scopes)
		{
            var exp = ((Value)Value).Contains("Evaluate").First();

			dynamic e = Engine;

	        e.sys.scop.FindValue
		        = new Func<ValueContainer, string, Value>(
			        (val, name) =>
			        {
				        return val.FindValue(name);
			        });

			e("Scopes.FindValue = (name) => sys.scop.FindValue(this, name)");
			
			scopes.NewStackBoundary();

			dynamic ss = scopes.NewValue();
			ss.@this = Value;

			scopes.Push(ss);

			dynamic scopeValue
				= scopes.NewValue(scopes, Engine.GlobalContainer.FindValue("Scopes"));

			var expression = ((FunctionAbstract) exp.RawValue).Lambda;

			var c = new FunctionCallExpression(expression, new List<Value>() {(Value)scopeValue});

		    c.Debugger = Debugger;

			var res = e.RunExpressionCustomScope(c, scopes);

			scopes.DeleteStackBoundary();

			return res;
		}

	    public Value Process(ValueContainer scopes)
	    {
	        return Evaluate(scopes);
	    }

	    public IList<IExpression> Children => null;

	    public IDebugger Debugger { get; set; }

	    public CodeLine CodeLine { get; set; }
	}
}