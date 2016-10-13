using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DotnetSpider.Core.Pipeline;
using DotnetSpider.Core.Processor;
using DotnetSpider.Core.Scheduler;

namespace DotnetSpider.Core
{
	/// <summary>
	/// Interface for identifying different tasks.
	/// </summary>
	public class DefaultSpider : Spider
	{
		public DefaultSpider(string id, Site site) : base(site, id, "admin", "", new DefaultPageProcessor(), new QueueDuplicateRemovedScheduler())
		{
		}
	}
}
