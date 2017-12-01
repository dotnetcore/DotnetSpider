using System;
using System.Collections.Generic;

namespace DotnetSpider.Core.Scheduler
{
    /// <summary>
    /// Scheduler is the part of url management. 
    /// You can implement interface Scheduler to do:
    /// manage urls to fetch
    /// remove duplicate urls 
    /// 负责URL的调度、去重，可以实现如Queue, PriorityQueueScheduler, RedisScheduler(可用于分布式)等等
    /// </summary>
    public interface IScheduler : IDisposable, IMonitorable, IClear
	{
		bool DepthFirst { get; set; }

		void Init(ISpider spider);

		/// <summary>
		/// Add a url to fetch
		/// </summary>
		/// <param name="request"></param>
		void Push(Request request);

        /// <summary>
        /// Get an url to crawl
        /// 获取要爬取的URL
        /// </summary>
        /// <returns></returns>
        Request Poll();

		void Import(HashSet<Request> requests);

		void Export();
	}
}
