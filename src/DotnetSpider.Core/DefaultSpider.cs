using System;
using DotnetSpider.Core.Processor;
using DotnetSpider.Core.Scheduler;

namespace DotnetSpider.Core
{
	/// <summary>
	/// Interface for identifying different tasks.
	/// </summary>
	public class DefaultSpider : Spider
	{
		public DefaultSpider() : this(Guid.NewGuid().ToString(), new Site())
		{
		}

		public DefaultSpider(string id, Site site) : base(site, id, "admin", "", new QueueDuplicateRemovedScheduler(), new SimplePageProcessor())
		{
		}
	}
}
