using System;
using System.IO;
using System.Net.Http;
using DotnetSpider.Core;
using DotnetSpider.Core.Common;
using DotnetSpider.Core.Pipeline;
using NLog;

namespace DotnetSpider.Extension.Model.Formatter
{
	public class Download : Formatter
	{
		private static readonly HttpClient Client = new HttpClient();

		private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

		public override string Formate(string value)
		{
			try
			{
				var name = Path.GetFileName(value);
				if (name != null)
				{
					var fileData = Client.GetByteArrayAsync(value).Result;
					string file = Path.Combine(SpiderEnviroment.GlobalDirectory, "images", name);
					if (File.Exists(file))
					{
						return value;
					}
					var stream = BasePipeline.PrepareFile(file).OpenWrite();
					foreach (var b in fileData)
					{
						stream.WriteByte(b);
					}
					stream.Flush();
					stream.Dispose();
				}
				return value;
			}
			catch (Exception e)
			{
				Logger.SaveLog(LogInfo.Create($"Download file: {value} failed.", Logger.Name, null, LogLevel.Error, e));
				throw;
			}

		}
	}
}
