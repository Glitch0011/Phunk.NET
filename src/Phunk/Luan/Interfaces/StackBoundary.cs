using System.Collections.Generic;
using Phunk.Values;

namespace Phunk.Luan.Interfaces
{
	public class StackBoundary : Stack<Value>
	{
		public Value Arguments { get; set; }
		public bool AccessingMember { get; set; }
        public IEngine Executor { get; set; }
	}
}