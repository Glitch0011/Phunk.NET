using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using Phunk.Luan;
using Phunk.Luan.Expressions;
using Phunk.Luan.Interfaces;
using System.Reflection;

namespace Phunk
{
    public class Value : DynamicObject, IConvertible
	{
        public Guid Guid;

		public Value(object obj = null, Value @class = null)
		{
            Guid = Guid.NewGuid();

			if (obj is Value)
			{
				Debugger.Break();
				RawValue = ((Value) obj).RawValue;
			}
			else
			{
				RawValue = obj;
			}
			Class = @class;
		}

		public object RawValue { get; set; }

	    public Value ConstructorClass { get; set; }

		public Value Class { get; set; }

		public Dictionary<string, Value> ObjectValues { get; set; } = new Dictionary<string, Value>();

		public IReadOnlyDictionary<string, Value> Values
		{
			get
			{
				IList<IReadOnlyDictionary<string, Value>> additionalMembers = new List<IReadOnlyDictionary<string, Value>>();

				if (Class != null)
					if (Class != this)
						additionalMembers.Add(Class.Values);
				
				//Load Value functions
				/*if (Engine?.Globals.ObjectValues.ContainsKey("Value") ?? false)
					additionalMembers.Add(Engine.Globals.ObjectValues["Value"].ObjectValues);*/

				try
				{
					IDictionary<string, Value> values= new Dictionary<string, Value>();

					//Load inherited variables
					foreach (var val in additionalMembers)
						foreach (var value in val)
							values[value.Key] = value.Value;

					//Prioritise local variables over class vars
					foreach (var value in ObjectValues)
						values[value.Key] = value.Value;

					return values.ToDictionary(x => x.Key, x => x.Value);
				}
				catch (Exception e)
				{
					return null;
				}
			}
		}

		public Value this[string index]
		{
			get
			{
				if (Values.ContainsKey(index))
					return Values[index];
				return null;
			}
			set
			{
                if (this[index] != null)
                {
                    ObjectValues[index] = value;
                }
                else
                {
                    ObjectValues.Add(index, value);
                }
			}
		}

		public bool IsFunction => RawValue is Delegate ||
		                          RawValue is Expression ||
		                          RawValue is FunctionAbstract;

		public bool IsNull => RawValue == null;

		public bool IsNumber => RawValue is int || RawValue is double;

		public bool HasConstructor => Values.ContainsKey("constructor");

		public Engine Engine { get; set; }

		public bool IsTrue
		{
			get
			{
				if (IsBool)
					return (bool) RawValue;
				if (IsNumber)
					return (int) RawValue != 0;

				return !IsNull;
			}
		}

		public bool IsBool => RawValue is bool;

		public bool IsReturning { get; set; }

		public bool IsArray => RawValue != null && (RawValue.GetType().HasElementType || RawValue is IList);

		//HACK
		public string LastName { get; set; }

		public List<Value> Classes
		{
			get
			{
				var items = new List<Value>();

				var trace = this.Class;

				while (trace != null)
				{
					items.Add(trace);
					trace = trace.Class;
				}

				return items;
			}
		}

		public TypeCode GetTypeCode()
		{
			throw new NotImplementedException();
		}

		public bool ToBoolean(IFormatProvider provider)
		{
			return GenericReturn<bool>();
		}

		public char ToChar(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public sbyte ToSByte(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public byte ToByte(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public short ToInt16(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public ushort ToUInt16(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public int ToInt32(IFormatProvider provider)
		{
			if (RawValue is int)
				return (int) RawValue;
			else if (RawValue is double)
				return (int) ((double) RawValue);
			else
				throw new NotSupportedException();
		}

		public uint ToUInt32(IFormatProvider provider)
		{
			return GenericReturn<uint>();
		}

		public long ToInt64(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public ulong ToUInt64(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public float ToSingle(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public double ToDouble(IFormatProvider provider)
		{
			if (RawValue is int)
				return (int) RawValue;
			else if (RawValue is double)
				return (double) RawValue;
			else if (RawValue is string)
				return double.Parse(RawValue.ToString());
			else
				return 0;
				//throw new NotSupportedException();
		}

		public decimal ToDecimal(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public DateTime ToDateTime(IFormatProvider provider)
		{
			throw new NotImplementedException();
		}

		public string ToString(IFormatProvider provider)
		{
			return RawValue?.ToString();
		}

		public override string ToString()
		{
			string raw = null;

			if (Engine != null)
			{
				var e = this.Contains("convert_string").FirstOrDefault();
				if (e != null)
				{
					var scopes = new ValueContainer(Engine);

					scopes.Push(Engine.Globals);
					scopes.NewStackBoundary();

					dynamic ss = new Value();
					ss.@this = this;

					scopes.Push(ss);

					dynamic scopeValue
						= new Value(scopes, Engine.GlobalContainer.FindValue("Scopes"));

					var expression = ((FunctionAbstract)e.RawValue).Lambda;

					var c = new FunctionCallExpression(expression, new List<Value>() { (Value)scopeValue });

					dynamic res = Engine.RunExpressionCustomScope(c, scopes);

					return (string) res;
				}	
			}

			if (IsNumber)
				raw = RawValue.ToString();

			if (RawValue is string)
				raw = $"\"{RawValue}\"";

            if (RawValue is Expression)
                raw = ((Expression)RawValue).ToString();

			if (IsBool)
				raw = $"{RawValue}";

            if (RawValue is FunctionAbstract)
            {
                raw = ((FunctionAbstract)RawValue).ToString();
                return raw;
            }

			if (RawValue is Delegate)
				raw = ((Delegate)RawValue).ToString();

			if (IsArray)
			{
				var v = (List<Value>) RawValue;
				raw = "[" + string.Join(",", v.Select(x => x.ToString())) + "]";
			}

			var left = raw;
			var right = (Values?.Any() ?? false) ? $"({string.Join(",", Values.Keys)})" : null;

			if (left != null && right != null)
				return $"{left}: {right}";

			if (left != null)
				return left;

			if (right != null)
				return right;

			if (RawValue.ToString() != RawValue.GetType().ToString())
			{
				return RawValue.ToString();
			}

			return "null";
		}

		public object ToType(Type conversionType, IFormatProvider provider)
		{
			if (conversionType == typeof(Value))
				return this;

			if (RawValue == null)
				return null;

			if (conversionType == typeof(int))
				return ToInt32(provider);

			if (conversionType == typeof(double))
				return ToDouble(provider);

			if (RawValue == null)
				throw new NullReferenceException();

			if (RawValue.GetType() == conversionType)
				return RawValue;

			return new TypeConverter().ConvertTo(RawValue, conversionType);
		}
		
		public Value GetNewOrExistingValue(string memberName, ValueContainer scopes)
		{
			if (ObjectValues == null)
				return null;

			if (!ObjectValues.ContainsKey(memberName))
				this[memberName] = scopes?.NewValue() ?? new Value();

			return this[memberName];
		}

		public Value GetNewValue(string memberName)
		{
			if (!Values.ContainsKey(memberName))
				this[memberName] = new Value();
			else
				throw new NotSupportedException();

			return this[memberName];
		}

		public IList<Value> ContainsWithTreeScan(string str)
		{
			var ret = new List<Value>();

			if (Values == null)
				return ret;

			foreach (var s in Values)
			{
				if (string.Equals(s.Key, str, StringComparison.CurrentCultureIgnoreCase))
				{
				    if (s.Value.IsNull)
				        ret.AddRange(s.Value.ObjectValues.Where(x => x.Value.IsFunction).Select(x => x.Value));
				    else
				        ret.Add(s.Value);
				}
			}

			return ret;

			if (Values.Keys.Select(x => x.ToLower()).Contains(str.ToLower()))
			{
				return Values.Where(x => string.Equals(x.Key, str, StringComparison.CurrentCultureIgnoreCase))
					.Select(x => x.Value)
					.ToList().SelectMany(x =>
					{
						if (x.IsNull)
							return x.Values.Where(y => y.Value.IsFunction).Select(y => y.Value);
						return new List<Value> {x};
					}).ToList();
			}

			return null;
		}

		public IList<Value> ContainsNoInheritance(string str)
		{
			return ContainsRaw(ObjectValues, str, StringComparison.CurrentCultureIgnoreCase);
		}

		public IList<Value> ContainsExactNoInheritance(string str)
		{
			return ContainsRaw(ObjectValues, str, StringComparison.CurrentCulture);
		}

		public IList<Value> Contains(string str)
		{ 
			return ContainsRaw(Values, str, StringComparison.CurrentCultureIgnoreCase);
		}

		public IList<Value> ContainsExact(string str)
		{
			return ContainsRaw(Values, str, StringComparison.CurrentCulture);
		}

		public IList<Value> ContainsRaw(IReadOnlyDictionary<string, Value> source, string str, StringComparison comparison)
		{
			if (source == null)
				return new List<Value>();

			var ret = new List<Value>();

			foreach (var s in source)
			{
				if (string.Equals(s.Key, str, comparison))
				{
					ret.Add(s.Value);
					//return s.Value;
				}
			}

			return ret;

			/*if (source.Keys.Select(x => x.ToLower()).Contains(str.ToLower()))
				return
					source.Where(x => string.Equals(x.Key, str, comparison))
						.Select(x => x.Value)
						.ToList();

			return new List<Value>();*/
		}

		public void Set(Value value)
		{
			RawValue = value?.RawValue;
			ObjectValues = value?.ObjectValues;
		    ConstructorClass = value?.ConstructorClass;
			Guid = value?.Guid ?? Guid.Empty;
			Class = value?.Class;
		}

		public void SetValue(object obj)
		{
			if (obj is Value)
			{
				RawValue = ((Value) obj).RawValue;
			}
			else
			{
				RawValue = obj;
			}
		}

		public static implicit operator bool(Value val)
		{
			return val.ToBoolean(null);
		}

		public object Convert(Type target)
		{
			if (target == null)
				return null;

			return GetType().GetMethod(nameof(GenericReturn)).MakeGenericMethod(target).Invoke(this, null);
		}

		public override bool TryConvert(ConvertBinder binder, out object result)
		{
			result = Convert(binder.Type);

			return true;
		}

		public T GenericReturn<T>()
		{
			if (RawValue is T)
				return (T) RawValue;
				
			if (RawValue is string)
			{
				return ((string) RawValue).Convert<T>();
			}

			//Is it a list
			if (typeof(T).HasElementType || typeof(IList).IsAssignableFrom(typeof(T)))
			{
				var subType = typeof(T).GetElementType() ?? typeof(T).GenericTypeArguments.FirstOrDefault();

				Array data;

				//Convert from RawData to Array
				if (RawValue is IList)
				{
					var vals = (IList) RawValue;
					data = Array.CreateInstance(subType, vals.Count);
					for (var i = 0; i < vals.Count; i++)
					{
						var val = (Value) vals[i];
						data.SetValue(val.ToType(subType, null), i);
					}
				}
				else
				{
					throw new NotSupportedException();
				}

				//Convert from Array to T

				if (typeof(T).IsArray)
				{
					dynamic typedData = Array.CreateInstance(subType, data.Length);
					data.CopyTo(typedData, 0);
					return typedData;
				}
				else if (typeof(IList).IsAssignableFrom(typeof(T)))
				{
					var listInstance = (IList) typeof(List<>)
						.MakeGenericType(subType)
						.GetConstructor(Type.EmptyTypes)
						?.Invoke(null);

					if (listInstance == null)
						throw new NotSupportedException();

					var converter = TypeDescriptor.GetConverter(subType);

					foreach (var i in data)
					{
						listInstance.Add(converter.ConvertTo(i, subType));
					}

					return (T)listInstance;
				}
				else
				{
					throw new NotSupportedException();
				}
			}

			try
			{
				return (T) ToType(typeof(T), null);
			}
			catch (NotSupportedException)
			{
				var valo = RawValue?.GetType().Name ?? RawValue?.GetType().Name ?? "null";

				throw new InvalidCastException(
					$"Cannot convert {valo} to {typeof(T).Name}");
			}
			catch (NullReferenceException)
			{
				throw new InvalidCastException(
					$"Cannot convert Null to {typeof(T).Name}");
			}
		}
		
		public static implicit operator double(Value val)
		{
			return val.GenericReturn<double>();
		}

		public override IEnumerable<string> GetDynamicMemberNames()
		{
			return Values.Keys;
		}

		public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
		{
			throw new NotSupportedException();
		}

		public override bool TrySetMember(SetMemberBinder binder, object value)
		{
			if (value is Value)
			{
				ObjectValues[binder.Name] = (Value) value;
			}
			else
			{
				var val = new Value(value);

				ObjectValues[binder.Name] = val;
			}
			return true;
		}

		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			result = GetNewOrExistingValue(binder.Name, null);
			//result = this[binder.Name];

			return true;
		}

		public void Delete(string key)
		{
			ObjectValues.Remove(key);
		}

		public string ToStringLight()
		{
			return (ObjectValues?.Any() ?? false) ? $"[{string.Join(",", ObjectValues.Keys)}]" : "[]";
		}
	}
}