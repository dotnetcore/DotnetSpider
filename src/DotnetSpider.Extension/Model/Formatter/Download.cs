using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using DotnetSpider.Core.Infrastructure;
using DotnetSpider.Core.Pipeline;

namespace DotnetSpider.Extension.Model.Formatter
{
	public class Download : Formatter
	{
		private static readonly HttpClient Client = new HttpClient
		{
			Timeout = new TimeSpan(0, 0, 2, 0)
		};

		protected override object FormateValue(object value)
		{
			var filePath = value.ToString();
			var name = Path.GetFileName(filePath);
			Task<byte[]> task = Client.GetByteArrayAsync(filePath);
			task.ContinueWith(t =>
			{
				if (t.Exception != null)
				{
					throw t.Exception;
				}
				var fileData = t.Result;
				string file = Path.Combine(Core.Env.GlobalDirectory, "images", name);
				if (!File.Exists(file))
				{
					var stream = FileUtil.PrepareFile(file).OpenWrite();
					foreach (var b in fileData)
					{
						stream.WriteByte(b);
					}
					stream.Flush();
					stream.Dispose();
				}
			});

			return name;
		}

		protected override void CheckArguments()
		{
		}
	}
}