using System;
using Phunk.Luan.Interfaces;
using Phunk.Values;

namespace Phunk.Luan
{
	public interface IEngine
	{
		Value NewValue(object raw = null, Value @class = null);
		Guid Id { get; set; }
		Value Globals { get; }
		ValueContainer GlobalContainer { get; }
        IEngine RootEngine { get; }
	}
}