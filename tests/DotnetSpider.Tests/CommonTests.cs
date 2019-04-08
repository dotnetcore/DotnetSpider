using System.Collections.Concurrent;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Xunit;

namespace DotnetSpider.Tests
{
	public class CommonTests : TestBase
	{
		[Fact]
		public void ConcurrentDictionary()
		{
			if (IsCI())
			{
				return;
			}

			ConcurrentDictionary<int, int> dict = new ConcurrentDictionary<int, int>();

			Task.Factory.StartNew(() =>
				{
					Parallel.For(0, 10000, i =>
					{
						dict.AddOrUpdate(i, i, ((i1, s) => s));
						Thread.Sleep(50);
					});
				})
				.ConfigureAwait(false).GetAwaiter();

			Task.Factory.StartNew(() =>
			{
				Parallel.For(0, 10000, (i) =>
				{
					var data = dict.Values;
					var result = 0;
					foreach (var item in data)
					{
						result += item;
					}

					Console.WriteLine(result);
					Thread.Sleep(50);
				});
			}).ConfigureAwait(false).GetAwaiter();
		}

		[Fact]
		public void LinqTake()
		{
			var list = new List<int>();
			var result = list.Take(10);
			Assert.Empty(list);
		}
	}
}