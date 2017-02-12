using Phunk.Luan.Interfaces;
using Phunk.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using Phunk.Luan.Exceptions;

namespace Phunk.Luan.Expressions
{
	public class ValueExpression : Expression
	{
		private string Value { get; }

		public string TrimmedValue => Value?.Trim();

		public ValueExpression(string value)
		{
			Value = value;
		}

		public override string ToString()
		{
			return Value?.Trim() ?? "null";
		}
        
        //The first stage is learning how to do something
        //The next, is learning how to solve a problem
        //The final stage is learning how to see a problem

        protected override Value Evaluate(ValueContainer scopes)
		{
            if (TrimmedValue == null)
				return scopes.NewValue(@class: "Null");

			if (TrimmedValue.ToLower() == "this")
				return GetLooseValue(scopes, "this");

	        if (TrimmedValue.ToLower() == "executor")
	        {
		        dynamic v = new Value();
		        v.Id = scopes.Engine.Id;
		        return v;
	        }

			var val = GetLooseValue(scopes, TrimmedValue);
			
			if (TrimmedValue.Trim() == "")
				return scopes.NewValue(@class: "Null");
				
			//We have a valid value that existed before
			if (val != null)
			{ 
				val.LastName = TrimmedValue;
				
				//Can't find NoReadList
				var noReadList = scopes.Engine.GlobalContainer.FindValue("NoReadList");

				if (noReadList?.RawValue != null)
				{
					//Find the no-read list
					IList<dynamic> v = ((List<Value>) noReadList.RawValue).Select(x => (dynamic) x).ToList();

					if (v.Any())
					{
						//Find any values in this list being called
						var vals = v.Where(x => x.Value.Guid.Equals(val.Guid)).ToList();
						
						//There is an item being monitored
						if (vals.Any())
						{
							//Get all the executor-ids allowed to access this value
							var exIds = vals.Select(x => x.ExecutorId);

                            var isNotOnAllowedList = exIds.FirstOrDefault(x => x.RawValue.Equals(scopes.Stack.Executor.Id)) == null;
                            var isNotRootEngine = !scopes.Stack.Executor.Id.Equals(scopes.Engine.RootEngine.Id);

                            //Check to see if we're on that list
                            if (isNotOnAllowedList && isNotRootEngine)
                            {
                                //We're not on the list, therefore not allowed to read it
                                throw new NoReadPermissionException($"{(scopes.Engine as Executable).Name} is not allowed to read {val.LastName}");
                            }
						}
					}
				}

				return val;
			}

            //Create new value on the top of the stack
            return scopes.Top?.GetNewOrExistingValue(TrimmedValue.Trim(), scopes) ?? scopes.NewValue(@class: "Null");
		}

		private static Value GetLooseValue(ValueContainer scopes, string val)
		{
			if (!scopes.Stack.AccessingMember)
				return scopes.FindValueExact(val) ?? scopes.FindValue(val);
			else
				return scopes.FindValueExactNoInheritance(val) ?? scopes.FindValueNoInheritance(val);
		}
	}
}