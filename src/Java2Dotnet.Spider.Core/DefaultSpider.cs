using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Java2Dotnet.Spider.Core.Pipeline;
using Java2Dotnet.Spider.Core.Processor;
using Java2Dotnet.Spider.Core.Scheduler;

using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using NLog;

namespace Java2Dotnet.Spider.Core
{
	/// <summary>
	/// Interface for identifying different tasks.
	/// </summary>
	public class DefaultSpider : ISpider
	{
		private static readonly Regex IdentifyRegex = new Regex(@"^[0-9A-Za-z_-]+$");

		public DefaultSpider() : this(Guid.NewGuid().ToString(), new Site())
		{
		}

		public DefaultSpider(string uuid, Site site)
		{
			if (!IdentifyRegex.IsMatch(uuid))
			{
				throw new SpiderException("Task Identify only can contains A-Z a-z 0-9 _ - [SPACE]");
			}
			Identity = uuid;
			Site = site;
			Logger = LogManager.GetCurrentClassLogger();
		}

		/// <summary>
		/// Unique id for a task.
		/// </summary>
		public string Identity { get; }

		/// <summary>
		/// Site of a task
		/// </summary>
		public Site Site { get; }

		public void Exit()
		{
		}

		public void Run()
		{
		}

		public void Stop()
		{
		}

		public Dictionary<string, dynamic> Settings { get; } = new Dictionary<string, dynamic>();

		public string UserId { get; } = "Default";

		public string TaskGroup { get; } = "Default";

		public ILogger Logger
		{
			get; set;
		}

		public IScheduler Scheduler
		{
			get; set;
		}

		public int ThreadNum
		{
			get; set;
		}

		public List<IPipeline> Pipelines
		{
			get; protected set;
		}

		public IPageProcessor PageProcessor
		{
			get; protected set;
		}

		public void Dispose()
		{
		}
	}
}
