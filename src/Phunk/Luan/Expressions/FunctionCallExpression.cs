using System;
using System.Collections.Generic;
using System.Linq;
using Phunk.Luan.Interfaces;
using Phunk.Values;
using System.Reflection;

namespace Phunk.Luan.Expressions
{
    internal class FunctionCallExpression : Expression
	{
		public string Function { get; }
		public IExpression FunctionExpression { get; }
		public string TrimmedFunction => Function?.Trim();

		public IList<IExpression> Arguments { get; }
		public IList<Value> ResolvedArguments { get; }

		public override IList<IExpression> Children
		{
			get
			{
				var v = new List<IExpression>();

				if (FunctionExpression != null)
					v.Add(FunctionExpression);

				v.AddRange(Arguments);

				return v;
			}
		}

		public FunctionCallExpression(string functionName, IList<IExpression> arguments)
		{
			Function = functionName;

			Arguments = arguments;
		}

		public FunctionCallExpression(string functionName, IList<Value> arguments)
		{
			Function = functionName;
			ResolvedArguments = arguments;
		}

		public FunctionCallExpression(Expression functionExpression, IList<Value> arguments)
		{
			FunctionExpression = functionExpression;
			ResolvedArguments = arguments;
		}

		public FunctionCallExpression(Expression functionExpression, IList<IExpression> arguments)
		{
			FunctionExpression = functionExpression;

			//Split by comma's, trim, then convert into expressions
			Arguments = arguments;
		}

		public override string ToString()
		{
			return
				$"{TrimmedFunction ?? FunctionExpression.ToString()}({string.Join(", ", Arguments.Select(x => x.ToString().Trim()))})";
		}

		public Value Execute(ValueContainer scopes, object obj)
		{
			Value[] arguments;

			if (ResolvedArguments != null)
			{
				arguments = ResolvedArguments.ToArray();
			}
			else
			{
				arguments = Arguments?.Select(x => x?.Process(scopes)).Where(x => x != null).ToArray();
			}
			
			if (obj.GetType() == typeof(Value))
			{
				var valueObj = (Value) obj;

				var constructors = valueObj.ContainsWithTreeScan("constructor");

				var possibleConstructor =
					constructors?.FirstOrDefault(x =>
					{
						if (x.RawValue is FunctionAbstract)
							return (x.RawValue as FunctionAbstract).Arguments.Count == arguments.Length;

						if (x.RawValue is Delegate)
							return new FunctionAbstract(x.RawValue).Arguments.Count == arguments.Length;

						if (!(x.RawValue is LambdaExpression))
							throw new NotSupportedException("Why is a constructor a value!");

						return ((LambdaExpression) x.RawValue).Arguments.Count == arguments.Length;
					});

				var functions = valueObj.Values.Where(x => x.Key.Trim() == TrimmedFunction && x.Value.IsFunction).Select(x => x.Value).ToList();
				
				if (valueObj.IsNull && functions.Any())
				{
					
					var possibleMethod =
						functions.FirstOrDefault(x =>
						{
							if (x.RawValue is FunctionAbstract)
								return (x.RawValue as FunctionAbstract).Arguments.Count == arguments.Length;

							if (!(x.RawValue is LambdaExpression))
								throw new NotSupportedException("Why is a constructor a value!");

							return ((LambdaExpression) x.RawValue).Arguments.Count == arguments.Length;
						});

					if (possibleMethod != null)
					{
						return InvokeSomething(scopes, arguments, possibleMethod.RawValue);
					}
					else
					{
						throw new NotSupportedException($"The method \"{TrimmedFunction}\" cannot be not found within scope!");
					}
				}
				else if (valueObj.IsFunction)
				{
					var func = valueObj.RawValue;

					if (func is Delegate)
					{
						return RetypeValue(InvokeMethod((Delegate) func, arguments), scopes);
					}
					if (func is Expression)
					{
						return InvokeLambda(scopes, arguments, (LambdaExpression) func);
					}
					if (func is FunctionAbstract)
					{
						return InvokeSomething(scopes, arguments, func);
					}
					throw new NotSupportedException();
				}
				else if (possibleConstructor != null)
				{
					//We should create a new context for the object

					var contextWithNewObject = scopes.Copy();

					var vel = scopes.Engine.NewValue(null, valueObj);
					contextWithNewObject.Push(vel);
				    vel.ConstructorClass = valueObj;
                    var constructorReturn = InvokeSomething(contextWithNewObject, arguments, possibleConstructor.RawValue);
					return vel;
				}
				else
				{
					throw new NotSupportedException($"Object {obj} is not executable!");
				}
			}

			throw new NotSupportedException();
		}

		private static Value RetypeValue(object v, ValueContainer scopes)
		{
			if (v == null)
				return null;

			if (v.GetType() == typeof(Value))
				return v as Value;

			Value newClass = null;

			if (v is int)
				newClass = scopes.FindValue("Number");

			if (v is double)
				newClass = scopes.FindValue("Number");

			if (v is string)
				newClass = scopes.FindValue("String");

			if (v.GetType().IsArray)
			{
				var vals = new List<Value>();
				
				foreach (var obj in (Array) v)
				{
					vals.Add(RetypeValue(obj, scopes));
				}
				
				return scopes.Engine.NewValue(vals, scopes.FindValue("Array"));
			}

			if (newClass?.IsFunction ?? false)
				newClass = InvokeSomething(scopes, null, newClass.RawValue);
			
			return scopes.Engine.NewValue(v, newClass);
		}

		public static Value InvokeSomething(ValueContainer scopes, Value[] arguments, object func)
		{
			if (func is FunctionAbstract)
			{
				var f = (FunctionAbstract) func;

				if (f.Raw is LambdaExpression)
				{
					return InvokeLambda(scopes, arguments, (LambdaExpression) f.Raw);
				}
				else if (f.Raw is Delegate)
				{
					return RetypeValue(InvokeMethod((Delegate) f.Raw, arguments), scopes);
				}
				else
				{
					throw new NotSupportedException();
				}
			}
			else if (func is LambdaExpression)
			{
				return InvokeLambda(scopes, arguments, (LambdaExpression) func);
			}
			else if (func is Delegate)
			{
				return RetypeValue(InvokeMethod((Delegate) func, arguments), scopes);
			}
			else
			{
				throw new NotSupportedException();
			}
		}

		public static Value InvokeLambda(ValueContainer scopes, Value[] arguments, LambdaExpression func)
		{
            //Okay, so we're about to invoke the lambda, now we should probably check to 
            //func.Executor

			var lowerContext = scopes.Copy();

            lowerContext.Engine = func.Executor;
            
			//Create context for function arguments
			lowerContext.NewStackBoundary(func.Executor);
			//lowerContext.Push(args);

			//Create context for function-level variables
			var stackBounary = lowerContext.NewStackBoundary();
			stackBounary.Arguments = ConstructorArgsValue(scopes, func, arguments, func.Arguments);
            stackBounary.Executor = func.Executor;
			lowerContext.Push(scopes.Engine.NewValue());

            var preId = scopes.Engine.Id;

            //scopes.
			var val = func.RightHandSide.Process(lowerContext);

			//Deset the returning flag
			if (val != null && val.IsReturning)
				val.IsReturning = false;

			return val;
		}

		private static Value ConstructorArgsValue(ValueContainer scopes, LambdaExpression func, Value[] arguments, IList<ArgumentDefinition> funcArguments)
		{
			if (func == null || arguments == null)
				throw new Exception("Func was null");

			//Construct value storing our context
			var args = scopes.Engine.NewValue(null, scopes.FindValue("Arguments"));

			var top = scopes.Top;

			var topThis = top["this"];

			if (topThis != null)
			{
				args["this"] = scopes.Top["this"];
			}
			else
			{
				//Set this context
				args["this"] = scopes.Top;
			}

			//Construct arguments values from incoming arguments
			for (var i = 0; i < Math.Min(arguments.Length, funcArguments.Count); i++)
				args[func.Arguments[i].Name] = arguments[i];
			return args;
		}

		private static object InvokeMethod(Delegate obj, object[] arguments)
		{
			var convertedArguments = new List<object>();

            var requiredParameters = obj.GetMethodInfo().GetParameters();

            if (arguments != null)
			{
				if (arguments.Length != requiredParameters.Length)
					throw new Exception($"Argument mismatch {arguments.Length} != {requiredParameters.Length}");

				for (var i = 0; i < arguments.Length; i++)
					convertedArguments.Add(Convert.ChangeType(arguments[i], requiredParameters[i].ParameterType));
			}

			return obj.DynamicInvoke(convertedArguments.ToArray());
		}

        protected override Value Evaluate(ValueContainer scopes)
		{
            //The expression is what defines the function
            var functionExpression = FunctionExpression ?? new ValueExpression(TrimmedFunction);
			
			//The value is the callable function itself
			var functionValue = functionExpression.Process(scopes);

			if (functionValue != null)
				return Execute(scopes, functionValue);

			throw new NotSupportedException();
		}
	}
}