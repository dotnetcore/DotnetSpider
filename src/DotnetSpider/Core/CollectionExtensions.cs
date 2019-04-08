using System;
using System.Collections.Generic;
using System.Linq;

namespace DotnetSpider.Core
{
	public static class CollectionExtensions
	{
		private static readonly Random RandomObject = new Random();

		public static T Random<T>(this ICollection<T> list)
		{
			var at = RandomObject.Next(0, list.Count - 1);
			return list.ElementAt(at);
		}
	}
}