using System;
using DotnetSpider.Common;
using System.IO;
using Serilog;
using Serilog.Events;

namespace DotnetSpider.Node
{
	class Program
	{
		static void Main(string[] args)
		{
			Log.Logger = new LoggerConfiguration()
				.MinimumLevel.Information()
#if !NET40
				.MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
#else
				.WriteTo.RollingFile(Path.Combine(Directory.GetCurrentDirectory(), "{Date}.log"))
#endif
				.WriteTo.Console()
				.CreateLogger();

			try
			{
				Console.Title = $"DOTNETSPIDER.NODE - {EnvironmentUtil.IpAddress} {NodeId.Id.ToUpper()} {Environment.ProcessorCount}";
			}
			catch
			{
				// IGNORE
			}
			var client = new NodeClient();
			// TODO: 需要考虑安全退出问题, 不然会导致 Block 丢失
			AppDomain.CurrentDomain.ProcessExit += (s, e) => { client.Dispose(); };
			client.Start();
			Console.Read();
		}
	}
}
