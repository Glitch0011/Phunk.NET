using System;
using System.Collections.Generic;
using System.Linq;
using Phunk.Values;

namespace Phunk.Luan.Interfaces
{
    public class ValueContainer
	{
		public IEngine Engine;

		private Stack<StackBoundary> StackBoundaries { get; } = new Stack<StackBoundary>();

		public StackBoundary Stack => StackBoundaries.FirstOrDefault();

		public Value Top => Stack?.FirstOrDefault();

		public ValueContainer(IEngine engine)
		{
			Engine = engine;
		}
		
		public ValueContainer(Value singleValue)
		{
			if (Stack == null)
				NewStackBoundary();

			Stack?.Push(singleValue);
		}

		public Value Find(Func<Value, IList<Value>> scan)
		{
			foreach (var stackBoundary in StackBoundaries.Where(x=>x != null))
			{
				foreach (var value in stackBoundary.Where(x => x != null))
				{
					var result = scan(value);

					if (result?.Count > 0)
						return result.First();
				}

                if (stackBoundary.Arguments != null)
                {
                    var arguments = scan(stackBoundary.Arguments);

                    if (arguments?.Count > 0)
                        return arguments.First();
                }
            }

			return null;
		}

		public Value FindValueNoInheritance(string name)
		{
			var v = Find(x => x?.ContainsNoInheritance(name));

			if (v != null)
				v.LastName = name;

			return v;
		}

		public Value FindValueExactNoInheritance(string name)
		{
			var v = Find(x => x.ContainsExactNoInheritance((name)));

			if (v != null)
				v.LastName = name;

			return v;
		}

		public Value FindValue(string name)
		{
			var v = Find(x => x?.Contains(name));

			if (v != null)
				v.LastName = name;

			return v;
		}

		public Value FindValueExact(string name)
		{
			var v = Find(x => x.ContainsExact(name));

			if (v != null)
				v.LastName = name;

			return v;
		}

		public ValueContainer Copy()
		{
			var container = new ValueContainer(Engine);

			foreach (var stackBoundary in StackBoundaries.Reverse())
			{
				var newStackBoundary = container.NewStackBoundary();

				newStackBoundary.Arguments = stackBoundary.Arguments;
                newStackBoundary.Executor = stackBoundary.Executor;
				newStackBoundary.AccessingMember = stackBoundary.AccessingMember;

				foreach (var scope in stackBoundary.Reverse())
					container.Push(scope);
			}

			return container;
		}

		public StackBoundary NewStackBoundary(IEngine executor = null)
		{
            //We use the existing last executor to define the next executor by default
            StackBoundaries.Push(new StackBoundary()
            {
                Executor = executor ?? StackBoundaries.FirstOrDefault()?.Executor ?? Engine
            });

			return StackBoundaries.First();
		}

		public void Push(Value scope)
		{
			if (Stack == null)
				NewStackBoundary();

			if (Stack == null)
				throw new NotSupportedException();

			Stack.Push(scope);
		}

		public bool IsCrossBoundary(Value left)
		{
			if (left == null)
				throw new NotSupportedException();

			var index = 0;

			foreach (var boundary in StackBoundaries)
			{
				if (boundary.Contains(left))
					break;

				if (boundary.SelectMany(x => x.Values.Values).ToList().Contains(left))
					break;

				index++;
			}

			if (index == 0)
				return false;

			return true;
		}

		public void DeleteStackBoundary()
		{
			StackBoundaries.Pop();
		}

		public Value NewValue()
		{
			return Engine?.NewValue(null, FindValue("Value"));
		}

		public Value NewValue(object raw = null, Value @class = null)
		{
			return Engine.NewValue(raw, @class);
		}

		public Value NewValue(object raw = null, string @class = null)
		{
			return Engine.NewValue(raw, FindValue(@class));
		}
	}
}