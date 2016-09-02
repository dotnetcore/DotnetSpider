using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DotnetSpider.Core.Pipeline;
using DotnetSpider.Core.Processor;
using DotnetSpider.Core.Scheduler;
using NLog;

namespace DotnetSpider.Core
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
#if NET_CORE
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
#endif

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

		public void Run(params string[] arguments)
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

		public Task RunAsync(params string[] arguments)
		{
			return Task.Factory.StartNew(() =>
			{
				Run(arguments);
			}).ContinueWith(t =>
			{
				if (t.Exception != null)
				{
					Logger.Error(t.Exception.Message);
				}
			});
		}
	}
}
