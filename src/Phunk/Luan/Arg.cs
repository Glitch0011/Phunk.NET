using System;
using Phunk.Values;

namespace Phunk.Luan
{
    internal class Arg : Tuple<string, Value>
	{
		public Arg(string item1, Value item2) : base(item1, item2)
		{

		}
	}
}