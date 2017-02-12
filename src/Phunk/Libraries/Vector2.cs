using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Phunk.Luan;

namespace Phunk.Libraries
{
	public class Vector2 : ILibrary
	{
		public string[] Code => new[]
		{
			"Vector2.constructor = (x, y) =>",
			"    this.x = x",
			"    this.y = y",

			"Vector2.+ = (Vector2 a, Vector2 b) =>",
			"    return Vector2(a.x + b.x, a.y + b.y)",

			"Vector2.- = (Vector2 a, Vector2 b) =>",
			"    return Vector2(a.x - b.x, a.y - b.y)",

			"Vector2.Length = () =>",
			"    return Math.Sqrt((this.x * this.x) + (this.y * this.y))",

			"Vector2.Distance = Vector2.Length"
		};

		public void RawFunctions(dynamic engine)
		{

		}
	}
}