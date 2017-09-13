
using System.IO;
using DotnetSpider.Core.Downloader;
using static DotnetSpider.Core.Test.SpiderTest;

namespace DotnetSpider.Core.Test
{
	public class Program
	{
		public static void Main(string[] args)
		{
			EnvironmentTest test = new EnvironmentTest();
			test.DefaultConfig();
			test.InsideConfig();
			test.OutsideConfig();
		}
	}
}
