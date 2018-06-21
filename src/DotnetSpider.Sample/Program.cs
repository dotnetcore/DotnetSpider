using DotnetSpider.Core;
using DotnetSpider.Extension;
using DotnetSpider.Sample.docs;
using MessagePack;

using System;
#if !NETCOREAPP
using System.Threading;
#else
using System.Text;
#endif

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
			Startup.Run(args);
		}


		/// <summary>
		/// <c>MyTest</c> is a method in the <c>Program</c>
		/// </summary>
		private static void MyTest()
		{

		}
	}
}
