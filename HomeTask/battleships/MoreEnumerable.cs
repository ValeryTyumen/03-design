using System;
using System.Collections.Generic;
using System.Linq;

namespace battleships
{
	public static class MoreEnumerable
	{
		public static IEnumerable<T> Generate<T>(Func<T> tryGetItem)
		{
			while (true)
				yield return tryGetItem();
			// ReSharper disable once FunctionNeverReturns
		}

		public static double Median(this IReadOnlyList<int> items)
		{
			return items.Count%2 == 1 ? items[items.Count/2 - 1] : (items[(items.Count - 1)/2] + items[(items.Count + 1)/2])/2.0;
		}
		
		public static IEnumerable<T> Repeat<T>(this Func<T> getItem)
		{
			while (true)
				yield return getItem();
		}

		public static IEnumerable<T> Select<T>(this IEnumerable<T> items, Action<T, int> process)
		{
			return items.Select((item, index) =>
			{
				process(item, index);
				return item;
			});
		}
	}
}