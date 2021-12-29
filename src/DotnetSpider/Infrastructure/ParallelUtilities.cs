using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace DotnetSpider.Infrastructure
{
	public static class ParallelUtilities
	{
		public static void For(int fromInclusive,
			int toExclusive, ExecutionDataflowBlockOptions options, Func<int, Task> body)
		{
			var actionBlock = new ActionBlock<int>(async i =>
			{
				await body(i);
			}, options);

			for (var i = fromInclusive; i < toExclusive; ++i)
			{
				actionBlock.Post(i);
			}

			actionBlock.Complete();
			actionBlock.Completion.Wait();
		}

		public static void Foreach<TSource>(IEnumerable<TSource> source, ExecutionDataflowBlockOptions options,
			Func<TSource, Task> body)
		{
			var actionBlock = new ActionBlock<TSource>(async i =>
			{
				await body(i);
			}, options);

			foreach (var item in source)
			{
				actionBlock.Post(item);
			}

			actionBlock.Complete();
			actionBlock.Completion.Wait();
		}
	}
}
