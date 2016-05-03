using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Java2Dotnet.Spider.Core
{
	/// <summary>
	/// Interface for identifying different tasks.
	/// </summary>
	public class DefaultSpider : ISpider
	{
		private static readonly Regex IdentifyRegex = new Regex(@"^[0-9A-Za-z_-]+$");
		public DefaultSpider(string uuid, Site site)
		{
			if (!IdentifyRegex.IsMatch(uuid))
			{
				throw new SpiderExceptoin("Task Identify only can contains A-Z a-z 0-9 _ - [SPACE]");
			}
			Identity = uuid;
			Site = site;
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

		public string UserId { get; } = "";

		public string TaskGroup { get; } = "";

		public void Dispose()
		{
		}
	}
}
