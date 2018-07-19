using DotnetSpider.Common;
using DotnetSpider.Core.Infrastructure;
using DotnetSpider.Core.Pipeline;
using DotnetSpider.Core.Processor;
using DotnetSpider.Core.Scheduler;
using System;

namespace DotnetSpider.Core
{
	/// <summary>
	/// 默认爬虫, 用于测试和一些默认情况使用, 框架使用者可忽略
	/// </summary>
	internal sealed class DefaultSpider : Spider
	{
		/// <summary>
		/// 构造方法
		/// </summary>
		public DefaultSpider() : this(Guid.NewGuid().ToString("N"), new Site())
		{
		}

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="id">爬虫标识</param>
		/// <param name="site">网站信息</param>
		public DefaultSpider(string id, Site site) : this(id, site, new QueueDuplicateRemovedScheduler())
		{
		}

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="id">爬虫标识</param>
		/// <param name="site">网站信息</param>
		/// <param name="scheduler">URL队列</param>
		public DefaultSpider(string id, Site site, IScheduler scheduler) : base(site, id, scheduler, new[] { new SimplePageProcessor() }, new[] { new ConsolePipeline() })
		{
			Logger = LogUtil.Create(id);
		}
	}
}
