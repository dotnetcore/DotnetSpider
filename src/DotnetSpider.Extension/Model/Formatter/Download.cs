using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using DotnetSpider.Core.Pipeline;
using DotnetSpider.Core.Infrastructure;

namespace DotnetSpider.Extension.Model.Formatter
{
	public class Download : Formatter
	{
		private static readonly HttpClient Client = new HttpClient
		{
			Timeout = new TimeSpan(0, 0, 2, 0)
		};

		protected override dynamic FormateValue(dynamic value)
		{
			var name = Path.GetFileName(value);
			if (name != null)
			{
				Task<byte[]> task = Client.GetByteArrayAsync(value);
				task.ContinueWith(t =>
				{
					if (t.Exception != null)
					{
						throw t.Exception;
					}
					var fileData = t.Result;
					string file = Path.Combine(SpiderConsts.GlobalDirectory, "images", name);
					if (!File.Exists(file))
					{
						var stream = BasePipeline.PrepareFile(file).OpenWrite();
						foreach (var b in fileData)
						{
							stream.WriteByte(b);
						}
						stream.Flush();
						stream.Dispose();
					}
				});
			}
			return name;
		}

		protected override void CheckArguments()
		{
		}
	}
}
