using System;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using DotnetSpider.Extension;
using DotnetSpider.Test.Example;
using DotnetSpider.Test.Pipeline;
using Newtonsoft.Json;
using DotnetSpider.Core.Downloader;
using System.Threading.Tasks;
using System.Threading;
using DotnetSpider.Core;
using System.Collections.Generic;
using DotnetSpider.Core.Common;
using System.Text.RegularExpressions;
using System.Linq;
using DotnetSpider.Extension.Configuration;
using DotnetSpider.Extension.Configuration.Json;

using DotnetSpider.Core.Monitor;
using DotnetSpider.Extension.Monitor;
using System.Net;
using DotnetSpider.Extension.ORM;
using DotnetSpider.Extension.Model.Attribute;
using DotnetSpider.Extension.Model;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetSpider.Test
{
	public class Program
	{
		public static void Main(string[] args)
		{
			IocExtension.ServiceCollection.AddSingleton<IMonitorService, NLogMonitor>();
			//IocExtension.ServiceCollection.AddSingleton<IMonitorService, HttpMonitor>();

			JdSkuSampleSpider spiderBuilder = new JdSkuSampleSpider();
			spiderBuilder.Run("rerun");
			//var end = DateTime.Now;
			//Console.WriteLine((end - start).TotalMilliseconds);
			//Console.Read();
			//SpiderExample.Run();
			//JdSkuSampleSpider spiderBuilder = new JdSkuSampleSpider();
			//var context = spiderBuilder.GetBuilder().Context;
			//ContextSpider spider = new ContextSpider(context);
			//spider.Run("rerun");
		}
	}
}
