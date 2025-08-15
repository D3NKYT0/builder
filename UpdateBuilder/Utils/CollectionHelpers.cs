using System.Collections.Generic;

namespace UpdateBuilder.Utils
{
	public static class CollectionHelpers
	{
		public static void AddRange<T>(this ICollection<T> destination, IEnumerable<T> source)
		{
			foreach (T item in source)
			{
				destination.Add(item);
			}
		}
	}
}
