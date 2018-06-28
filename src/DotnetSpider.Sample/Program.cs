using DotnetSpider.Core;
using DotnetSpider.Extension;
using DotnetSpider.Sample.docs;
using MessagePack;
using MySql.Data.MySqlClient;
using System;
using System.Text.RegularExpressions;
#if !NETCOREAPP
using System.Threading;
#else
using System.Text;
#endif
using Dapper;
using DotnetSpider.Extension.Infrastructure;

namespace DotnetSpider.Sample
{
	public class Program
	{
		public static void Main(string[] args)
		{
#if NETCOREAPP
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
#else
			ThreadPool.SetMinThreads(200, 200);
			OcrDemo.Process();
#endif
			CrawlerWholeSiteSpider.Run();
		}

		class WuQi
		{
			public string Summary { get; set; }
			public int Id { get; set; }
		}


		/// <summary>
		/// <c>MyTest</c> is a method in the <c>Program</c>
		/// </summary>
		private static void MyTest()
		{

		}
	}
}
