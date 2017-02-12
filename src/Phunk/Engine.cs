using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using Phunk.Libraries;
using Phunk.Luan.Expressions;
using Phunk.Luan.Interfaces;
using Phunk.Values;
using System.Reflection;
using Phunk.Luan;

namespace Phunk
{
	public class Engine : Executable
	{
		[DebuggerHidden]
		public override Value Globals { get; }

		[DebuggerHidden]
		public override ValueContainer GlobalContainer => new ValueContainer(Globals);
        
		public event Action<string> OnConsoleOutput;

		public bool IsDebugging { get; set; } = false;
		
		public Engine(bool slim = false)
		{
			Globals = NewValue();
			
			Globals["Arguments"] = new Value(null, null);

			LoadValue();

			D.@true = NewValue(true);
			D.@false = NewValue(false);

			//D.Arguments = NewValue();
			D.Null = NewValue();
            D.Null.Name = "I'm a null";
		
			LoadValueExpression();

			LoadConsole();
			LoadString();
			LoadNumbers();
			LoadStringMath();
			
			LoadStringExpression();

            var random = new Random(DateTime.Now.Millisecond);

            D.Random = new Func<double, double, double>((min, max) =>
            {
                return random.NextDouble() * (max - min) + min;
            });
            
			if (!slim)
			{
				LoadMaths();
				LoadArray();

				D("Keywords = Array()");

				LoadNewValueExpression();
				LoadNewKeyword();

				LoadNoReadExpression();

                D("Regexes = Array()");

                D(new[]
                {
                   "GetNoReadRegex = () =>",
                   "    NoReadKeyword.Regex = \"no_read (?<Value>.*)\"",
                   "    NoReadKeyword.Replace = \"no_read_f(${Value}, Executor.Id)\"",
                   "    return NoReadKeyword",

                   "Regexes.Push(GetNoReadRegex())",
               });
            }

            IsDebugging = true;
			AttachDebugger();

			
		}

		private void LoadNoReadExpression()
		{
			//absolutum dominium
			D.sys.Exec = new Func<string, Value, Value>((str, scopes) => D(str));

            D.sys.Split = new Func<string, IExpression>((str) =>
            {
                return new ExpressionBuilder(this.GetExecuter()).Split(new CodeLine(str));
            });

            D(new[]
            {
                "no_read_f = (val, i) =>",
                "    noReadVal.ExecutorId = i",
                "    noReadVal.Value = val",
                "    NoReadList += noReadVal",
            });

			D(new[]
			{
				"NoReadList = Array()",

				"NoReadExpression.constructor = (str) =>",
				"    this.TargetValue = str",
				"NoReadExpression.Evaluate = (scopes, rhs) =>",
				"    val.ExecutorId = Executor.Id",
				"    val.Value = sys.Exec(this.TargetValue, scopes))",
				"    NoReadList += val",

				"GetNoReadKeyword = () =>",
				"    NoReadKeyword.Keyword = \"no_read\"",
                "    NoReadKeyword.Split = (raw) =>",
				"        s = sys.Split(raw)",
				"        return NoReadExpression(s)",
				"    return NoReadKeyword",

				"Keywords.Push(GetNoReadKeyword())",
			});
		}

		private void LoadValueExpression()
		{
			D.sys.ValueExpression.SetRaw = new Action<Value, string>(
				(a, b) => a.RawValue = new ValueExpression(b));

			D.sys.ValueExpression.Evaluate = new Func<Value, ValueContainer, Value>(
				(a, b) => (a.RawValue as Expression)?.Process(b));

			D(new[]
			{
				"ValueExpression.constructor = (exp) =>",
				"     sys.ValueExpression.SetRaw(this, exp)",
				"ValueExpression.Evaluate = (scope) =>",
				"    return sys.ValueExpression.Evaluate(this, scope)"
			});
		}

		private void LoadNewKeyword()
		{
			D(new[]
			{
				"GetNewKeyword = () =>",
				"    newKeyword.Keyword = \"new\"",
				"    newKeyword.Split = (raw) =>",
				"        raw = raw.Trim()",
				"        return NewValueExpression(raw)",
				"    return newKeyword",
				"",
				"Keywords.Push(GetNewKeyword())",
			});
		}

		private void LoadNewValueExpression()
		{
			D.sys.NewValueExpression.SetRaw = new Action<Value, string>(
					(a, b) => a.RawValue = new ValueExpression(b));

			D.sys.NewValueExpression.Evaluate = new Func<Value, ValueContainer, Value>(
				(a, b) => (a.RawValue as Expression)?.Process(b));

			D(new[]
			{
				"NewValueExpression.constructor = (exp) =>",
				"     sys.NewValueExpression.SetRaw(this, exp)",
			});

			D(new[]
			{
				"NewValueExpression.Evaluate = (scope) =>",
				"    return Value(sys.NewValueExpression.Evaluate(this, scope)).Copy()"
			});
		}

		private void LoadStringExpression()
		{
			D(new []
			{
				"StringExpression.constructor = (str) =>",
				"    this.StringValue = str",
				"StringExpression.Evaluate = (scopes) =>",
				"    return Value(this.StringValue, scopes.FindValue(\"String\"))"
			});
		}

		private void LoadValue()
		{
			this.Globals["Value"] = new Value(null, null);

			D(new[]
			{
				"Value.constructor = (val) =>",
				"    sys.val_l.Set(this, val)",
			});

			D.sys.val_l.Set = new Action<Value, Value>((value, o) => value.Set(o));
			D.sys.val_l.SetRaw = new Action<Value, Value>((value, o) => value.SetValue(o));
			D.sys.val_l.GetRaw = new Func<Value, object>(value => value.RawValue);
			D.sys.val_l.GetClass = new Func<Value, Value>(value => value.ConstructorClass);

			D.sys.val_l.SetClass = new Action<Value, Value>((value, s) => value.Class = s);

			D.sys.val_l.Value = new Func<Value, Value>(val => val);

			D.sys.NewVal = new Func<object, Value, Value>((val, newClass) => NewValue(val, newClass));

			D(new[]
			{
				"Value.constructor = (val, class) =>",
				"    sys.val_l.Set(this, val)",
				
				"Value.Copy = () =>",
				"    return Value(sys.val_l.GetRaw(this), sys.val_l.GetClass(this))",

				"Value.SetRaw = (val) =>",
				"    sys.val_l.SetRaw(this, val)",

                "Value.GetRaw = () =>",
                "    sys.val_l.GetRaw(this)",

				"Value.SetClass = (val) =>",
				"    sys.val_l.SetClass(this, val)",

                "Value.GetClass = () =>",
                "    sys.val_l.GetClass(this)",
			});

			D(new[]
			{
				"Value.GetValue = () =>",
				"    retun this.Val",
			});
		}

		private void LoadConsole()
		{
			D.Console.Write = new Action<string>(str => OnConsoleOutput?.Invoke(str));
            D.Console = new Action<string>(str => OnConsoleOutput?.Invoke(str));

            D.Debug = new Action<Value>((val) =>
            {
                System.Diagnostics.Debugger.Break();
            });

            /*D.Debug = new Action(() =>
            {
                System.Diagnostics.Debugger.Break();
            });*/
        }

		private void LoadString()
		{
			D.String = NewValue();

			D.sys.str.Trim = new Func<string, string>(str => str.Trim());
			D.sys.str.Split = new Func<string, string, string[]>((str, split) => str.Split(new[] { split }, StringSplitOptions.RemoveEmptyEntries));

			D("String.Trim = () => sys.str.Trim(this)");
			D("String.Split = (char) => sys.str.Split(this, char)");
		}

		private void LoadStringMath()
		{
			D.sys.AddString = new Func<string, string, string>((a, b) => a + b);

			D("String.+ = (String a, String b) => sys.AddString(a, b)");
			D("String.+ = (String a, Number b) => a + b.ToString()");
		}

		[DebuggerHidden]
		private dynamic D => this;

        [DebuggerStepThrough]
        public dynamic GetExecuter(Value globalOverride = null)
	    {
		    var e = new EngineExecuter(this, globalOverride);

		    Debugger?.OnEngineExecutorStart(e);

		    e.OnDispose = () => Debugger?.OnEngineExecutorFinish(e);

			return e;
	    }

		private void LoadNumbers()
		{
			D.sys.NumberToString = new Func<int, string>((a) => a.ToString());

			D.Number = NewValue();

			D(new[]
			{
				"Number.constructor = (val) =>",
				"    this.SetRaw(val)"
			});

			D("Number.ToString = () => sys.NumberToString(this)");
		}

		private void LoadMaths()
		{
			D.sys.Add = new Func<double, double, double>((a, b) => a + b);
			D.sys.Subtract = new Func<double, double, double>((a, b) => a - b);
			D.sys.Multiply = new Func<double, double, double>((a, b) => a*b);
			D.sys.Divide = new Func<double, double, double>((a, b) => a/b);
			D.sys.GreaterThan = new Func<double, double, bool>((a, b) => a > b);
			D.sys.LessThan = new Func<double, double, bool>((a, b) => a < b);

			D("Number.+ = (Number a, Number b) => sys.Add(a, b)");
			D("Number.- = (Number a, Number b) => sys.Subtract(a, b)");
			D("Number./ = (Number a, Number b) => sys.Divide(a, b)");
			D("Number.* = (Number a, Number b) => sys.Multiply(a, b)");
			D("Number.> = (Number a, Number b) => sys.GreaterThan(a, b)");
			D("Number.< = (Number a, Number b) => sys.LessThan(a, b)");

			//Define subtraction
			D(new[]
			{
				"Number.- = (Null a, Number b) => sys.Subtract(0, b)"
			});

			D.Math.Sqrt = new Func<double, double>(d => Math.Sqrt(d));
		}
		
		private void LoadArray()
		{
			D.sys.Array.SetRaw = new Action<Value>((child) =>
			{
				var list = new List<Value>();

				child.SetValue(list);
			});

			D.sys.Array.First = new Func<Value, Value>((a) =>
			{
				var vals = (List<Value>) a.RawValue;
				return vals.First();
			});

			D.sys.Array.Second = new Func<Value, Value>((a) =>
			{
				var vals = (List<Value>)a.RawValue;
				return vals.Skip(1).First();
			});

			D.sys.Array.Push = new Func<Value, Value, Value>((a, val) =>
			{
				if (a.RawValue == null)
					a.RawValue = new List<Value>();

				var vals = (List<Value>) a.RawValue;
				
				vals.Add(val);
				return val;
			});

			D.sys.Array.Length = new Func<Value, int?>((a) =>
			{
				var vals = (List<Value>)a.RawValue;
				return vals?.Count ?? null;
			});

			D.sys.Array.Take = new Func<Value, int, Value>((value, amount) =>
			{
				var vals = (List<Value>) value.RawValue;

				var items = vals.Take(amount).ToList();

				var val = NewValue(items, this["Array"]);

				return val;
			});

			D.sys.Array.Union = new Func<Value, Value, Value>((a, b) =>
			{
				var A = (List<Value>) a.RawValue;
				var B = (List<Value>) b.RawValue;

				if (A != null && B != null)
					return NewValue(A.Concat(B).ToList(), this["Array"]);
				else
					return null;
			});

			D.sys.Array.Index = new Func<Value, int, Value>((value, i) =>
			{
				var vals = (List<Value>) value.RawValue;

                if (i < vals.Count)
                    return vals?.ElementAt(i);
                else
                    throw new Exception($"Element {i} is larger than the array size of {vals.Count}");
			});

			D.sys.Array.Where = new Func<Value, Value, Value>((a, b) =>
			{
				throw new NotSupportedException();
			});

			D.Sys.Array.RemoveArray = new Action<Value, Value>((array, toRemove) =>
			{
				var listA = (List<Value>) array.RawValue;
				var listB = (List<Value>)toRemove.RawValue;

				foreach (var item in listB)
					listA.Remove(item);
			});

			D(new[]
			{
				"Array.constructor = () =>",
				"    sys.Array.SetRaw(this)"
			});

			D(new[]
			{
				"Array.First = () => sys.Array.First(this)",
				"Array.Second = () => sys.Array.Second(this)",
				"Array.Push = (val) => sys.Array.Push(this, val)",
				"Array.Add = (val) => sys.Array.Push(this, val)",
				"Array.Length = () => sys.Array.Length(this)",
				"Array.Take = (amount) => sys.Array.Take(this, amount)",
				"Array.Get = (index) => sys.Array.Index(this, index)",

				"Array.+ = (Array a, Array b) => sys.Array.Union(a, b)",

				"Array.+ = (Array a, b) =>",
				"    a.Push(b)",
				"    return a",

                "Array.Join = (delimiter) =>",
                "    str = this.Get(0).ToString()",
                "    for (i = 1; i < this.Length(); i++)",
                "        str += delimiter",
                "        str += this.Get(i).ToString()",
                "    return str",
			});
			
			D(new[]
			{
				"Array.Where = (cond) =>",
				"    results = Array()",
				"    for (i=0;i<this.Length();i++)",
				"        if (cond(this.Get(i)))",
				"            results.Push(this.Get(i))",
				"    return results",

				"Array.ForAll = (evt) =>",
				"    res = Array()",
				"    for (i=0;i<this.Length();i++)",
				"        res.Push(evt(this.Get(i)))",
				"    return res",

				"Array.Find = (cond) =>",
				"    ret = null",
				"    for (i=0;i<this.Length();i++)",
				"        if (cond(this.Get(i)))",
				"            ret = this.Get(i)",
				"    return ret",

				"Array.Remove = (cond) =>",
				"    toRemove = Array()",
				"    for (i=0;i<this.Length();i++)",
				"        if (cond(this.Get(i)))",
				"            toRemove.Push(this.Get(i))",
				"    sys.Array.RemoveArray(this, toRemove)",

                "Array.RemoveArray = (a) =>",
                "    sys.Array.RemoveArray(this, a)",

				"Array.All = (cond) =>",
				"    results = this.ForAll(cond)",
				"    anyFalse = results.Find(x=>x == false)",
				"    res = true",
				"    if (anyFalse != null)",
				"        res = false",
				"    return res",

                "Array.Contains = (e) =>",
                "    return this.Find(x => x == e) != null)"
			});
		}

		public override Value NewValue(object raw = null, Value @class = null)
		{
			return new Value(raw, @class ?? this["Value"]) 
			{
				Engine = this
			};
		}
		
		protected override object RunLines(IEnumerable<CodeLine> code, Tuple<string, Value>[] args = null)
		{
			object lastVal = null;

            using (var e = GetExecuter())
			{
                e.Id = Id;

				foreach (var line in code)
				{
					lastVal = e.RunLine(line, args);
				}
			}

			return lastVal;
		}

		public static Engine operator +(Engine a, Type b)
		{
			if (!typeof(ILibrary).IsAssignableFrom(b))
				throw new Exception("Not a code library");

			var c = (ILibrary) Activator.CreateInstance(b);

			a.RunLines(c.Code);

			c.RawFunctions(a);

			return a;
		}

		public IDebugger Debugger { get; set; }

        public override IEngine RootEngine => this;

        public void AttachDebugger()
		{
			Debugger = new PhunkDebugger();
		}
	}
}