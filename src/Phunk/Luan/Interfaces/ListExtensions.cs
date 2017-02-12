using System.Collections.Generic;
using System.Linq;

namespace Phunk.Luan.Interfaces
{
    internal static class ListExtensions
	{
		public static T Pull<T>(this List<T> list) where T : class
		{
			var first = list.FirstOrDefault();
			if (first != null)
			{
				list.Remove(first);
				return first;
			}
			return null;
		}
	}
}