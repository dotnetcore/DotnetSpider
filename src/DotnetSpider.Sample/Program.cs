using DotnetSpider.Core;
using DotnetSpider.Sample.docs;
using System.Threading;

namespace DotnetSpider.Sample
{
	class Program
	{
		static void Main(string[] args)
		{
#if NETCOREAPP
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
#else
			ThreadPool.SetMinThreads(256, 256);
#endif
			
			ModelSpider.Run();
		}

		/// <summary>
		/// <c>MyTest</c> is a method in the <c>Program</c>
		/// </summary>
		private static void MyTest()
		{

		}
	}
}
