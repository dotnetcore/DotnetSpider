using System.Net;
using DotnetSpider.Core.Infrastructure;
using DotnetSpider.Downloader;
using Xunit;

namespace DotnetSpider.Core.Test
{
	public class HttpExecuteRecordTest
	{
		[Fact(DisplayName = "Record")]
		public void Record()
		{
			if (Env.IsWindows)
			{
				var result = HttpClientDownloader.Default.GetAsync("http://localhost:30013").Result;
				if (result.StatusCode == HttpStatusCode.OK)
				{
					Env.HubServiceTaskApiUrl = "http://localhost:30013/api/v1.0/task";

					var recorder = new HttpExecuteRecord(LogUtil.Create("aa"));
					recorder.Add("1", "test", "abcd");
					recorder.Remove("1", "test", "abcd");
				}
			}
		}
	}
}