using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using DotnetSpider.DataFlow;
using DotnetSpider.Downloader;

namespace DotnetSpider.Scheduler
{
	/// <summary>
	/// 基于内存的广度优先调度(去重 URL)
	/// </summary>
	public class QueueDistinctBfsScheduler : DuplicateRemovedScheduler
	{
		internal readonly ConcurrentDictionary<string, List<Request>> Requests =
			new ConcurrentDictionary<string, List<Request>>();

		/// <summary>
		/// 重置去重器
		/// </summary>
		public override void ResetDuplicateCheck()
		{
			DuplicateRemover.ResetDuplicateCheck();
		}

		/// <summary>
		/// 如果请求未重复就添加到队列中
		/// </summary>
		/// <param name="request">请求</param>
		protected override void PushWhenNoDuplicate(Request request)
		{
			if (!Requests.ContainsKey(request.OwnerId))
			{
				Requests.TryAdd(request.OwnerId, new List<Request>());
			}

			Requests[request.OwnerId].Add(request);
		}

		/// <summary>
		/// 从队列中取出指定爬虫的指定个数请求
		/// </summary>
		/// <param name="ownerId">爬虫标识</param>
		/// <param name="count">出队数</param>
		/// <returns>请求</returns>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public override Request[] Dequeue(string ownerId, int count = 1)
		{
			Check.NotNull(ownerId, nameof(ownerId));
			if (!Requests.ContainsKey(ownerId))
			{
				return new Request[0];
			}

			var requests = Requests[ownerId].Take(count).ToArray();
			if (requests.Length > 0)
			{
				Requests[ownerId].RemoveRange(0, requests.Length);
			}

			return requests;
		}
	}
}