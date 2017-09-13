using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotnetSpider.Core
{
	public abstract class AppBase : INamed, IRunable, IIdentity, ITask
	{
		public string Identity { get; set; }

		public string Name { get; set; }

		public string TaskId { get; set; }

		public Task RunAsync(params string[] arguments)
		{
			return Task.Factory.StartNew(() =>
			{
				Run(arguments);
			});
		}

		public abstract void Run(params string[] arguments);
	}
}
