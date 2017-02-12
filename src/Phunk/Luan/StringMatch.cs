using System.Text.RegularExpressions;

namespace Phunk.Luan
{
	internal static class StringMatch
	{
		public static Match Match(this string str, string regex)
		{
			var m = new Regex(regex).Match(str);
			return m.Length != 0 ? m : null;
		}
	}
}