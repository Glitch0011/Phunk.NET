using System.Collections.Generic;
using System.ComponentModel;

namespace Phunk.Luan
{
	public static class StringExtensions
	{
		public static string Rejoin(this IEnumerable<char> e)
		{
			return string.Join("", e);
		}

		public static T Convert<T>(this string input)
		{
			var converter = TypeDescriptor.GetConverter(typeof(T));

			return (T) converter.ConvertFromString(input);
		}
	}
}