using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using Phunk.Luan.Expressions;
using Phunk.Luan.Interfaces;
using Phunk.Values;
using System.Reflection;

namespace Phunk.Luan
{
	public abstract class Executable : DynamicObject, IEngine
	{
        public string Name { get; set; } = "Default";

        public Guid Id { get; set; } = Guid.NewGuid();

        public IEngine _rootEngine;

        public abstract IEngine RootEngine { get; }

        public override IEnumerable<string> GetDynamicMemberNames()
		{
			return this.Globals.GetDynamicMemberNames();
		}
		
		public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
		{
			var preMeth = GetType().GetMethod(binder.Name);

			if (preMeth != null)
			{
				result = preMeth.Invoke(this, args);
				return true;
			}

			return base.TryInvokeMember(binder, args, out result);
		}

		[DebuggerStepThrough]
		public object RunLines(IEnumerable<string> code, Tuple<string, Value>[] args = null)
		{
			return RunLines(code.Select(x =>
			{
				/*var stackFrame = new StackTrace(3, true).GetFrame(1);
				var fileName = stackFrame.GetFileName();
				var methodName = stackFrame.GetMethod().ToString();
				var lineNumber = stackFrame.GetFileLineNumber();*/

                //TODO Add support for this

				return new CodeLine(x)
				{
					//File = fileName,
					//LineNumber = lineNumber
				};
			}), args);
		}

		public abstract object RunLines(IEnumerable<CodeLine> code, Tuple<string, Value>[] args = null);

		[DebuggerStepThrough]
		public override bool TryInvoke(InvokeBinder binder, object[] args, out object result)
		{
			var code = new List<string>();

			var possibleCode = args.FirstOrDefault();

			if (possibleCode is Value && ((Value)possibleCode).RawValue is FunctionAbstract)
			{
				if (args.Length == 1)
				{
					result = RunExpression((IExpression)args.First());
				}
				else if (args.Length > 1)
				{
					var expression = ((FunctionAbstract)((Value)args.First()).RawValue).Lambda;

					//This needs to convert Values to use correct scopes and classes!
					IList<Value> blankArgs = args.Skip(1).Select(x =>
					{
						string newClass = null;

						if (x is CodeLine)
							x = ((CodeLine)x).Raw;

						if (x is string)
							newClass = "String";
						else if (x is int)
							newClass = "Number";
						else if (x is Value)
							return (Value)x;

						return NewValue(x, newClass != null ? new ValueContainer(Globals).FindValue(newClass) : null);
					}).OfType<Value>().ToList();

					var c = new FunctionCallExpression(expression, blankArgs);

					result = RunExpression(c, null);
				}
				else
				{
					throw new NotSupportedException();
				}
			}
			else
			{
				if (possibleCode is string[])
				{
					code.AddRange((string[])args.First());
				}
				else if (possibleCode is string)
				{
					code.Add((string)args.FirstOrDefault());
				}

				if (args.Length == 1)
				{
					result = RunLines(code);
				}
				else if (args.Length > 1)
				{
					var inArgs = args.Skip(1).ToList();

					var outArgs =
						inArgs.OfType<Tuple<string, Value>>()
							.Union(inArgs.OfType<KeyValuePair<string, Value>>().Select(x => new Tuple<string, Value>(x.Key, x.Value)))
							.ToArray();

					result = RunLines(code, outArgs);
				}
				else
				{
					throw new NotSupportedException();
				}
			}

			return true;
		}

		public abstract Value Globals { get; }
		public abstract ValueContainer GlobalContainer { get; }

        public Value OverrideGlobal { get; set; }

        public object RunExpression(IExpression exp, params Tuple<string, Value>[] args)
		{
			if (args != null)
			{
				foreach (var arg in args)
				{
					Globals[arg.Item1] = arg.Item2;
				}
			}
            
			try
			{
				var contexts = new ValueContainer(this);
                
                contexts.Push(Globals);
                
                if (OverrideGlobal != null)
                    contexts.Push(OverrideGlobal);

                /*if (Globals["this"] == null)
                    Globals["this"] = Globals;*/
                
                contexts.Top["this"] = contexts.Top;
                
                return Evaluate(contexts, exp);
			}
			finally
			{
				if (args != null)
				{
					foreach (var arg in args)
					{
						Globals.Delete(arg.Item1);
					}
				}
			}
		}

		public object RunExpressionCustomScope(IExpression exp, ValueContainer scope)
		{
			return Evaluate(scope, exp);
		}

		private object Evaluate(Value context, IExpression command)
		{
			return Evaluate(context, new List<IExpression> { command });
		}

		private object Evaluate(Value context, IList<IExpression> command)
		{
			if (context["this"] == null)
				context["this"] = context;

			return Evaluate(new ValueContainer(context), command);
		}

		private object Evaluate(ValueContainer context, IExpression command)
		{
			return Evaluate(context, new List<IExpression> { command });
		}

		private object Evaluate(ValueContainer context, IEnumerable<IExpression> command)
		{
			object lastVal = null;

			foreach (var exp in command)
				lastVal = exp?.Process(context);

			return lastVal;
		}

		public abstract Value NewValue(object raw = null, Value @class = null);
        
        public Value this[string index]
        {
            get
            {
                return Globals?[index];
            }
            set
            {
                var a = new List<CodeLine>() { new CodeLine($"this.{index} = argA") };
                var b = new List<Tuple<string, Value>>() { new Tuple<string, Value>("argA", value) }.ToArray();

                RunLines(a, b);
            }
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (value is Value)
            {
                this[binder.Name] = (Value)value;
            }
            else
            {
                this[binder.Name] = NewValue(value);
            }

            return true;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            var preMeth = GetType().GetMethod(binder.Name);
            if (preMeth != null)
            {
                result = preMeth;
                return true;
            }

            var name = binder.Name;

            result = RunLines(new[] { $"{name}" });

            return true;
        }
    }
}