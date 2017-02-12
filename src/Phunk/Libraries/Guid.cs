using System;
using Phunk.Luan;

namespace Phunk.Libraries
{
	public class Guid : ILibrary
	{
		public string[] Code => new[]
		{
			"Guid.constructor = (str) =>",
			"    this.SetRaw(str)",

			"Guid.NewValue = () =>",
			"    return Guid(sys.Guid.NewGuid())",

            "Guid.ToString = () =>",
            "    return this.GetRaw()",

			"Guid.NewGuid = Guid.NewValue",
		};

		public void RawFunctions(dynamic engine)
		{
			engine.sys.Guid.NewGuid = new Func<string>(() => System.Guid.NewGuid().ToString());
		}
	}
}