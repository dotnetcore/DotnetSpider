using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using DotnetSpider.Core.Infrastructure;
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
				var result = HttpSender.Request((new HttpRequest
				{
					Url = "http://localhost:30013"
				}));
				if (result.StatusCode == HttpStatusCode.OK)
				{
					Env.HunServiceTaskApiUrl = "http://localhost:30013/api/v1.0/task";

					var recorder = new HttpExecuteRecord();
					recorder.Add("1", "test", "abcd");
					recorder.Remove("1", "test", "abcd");
				}
			}
		}
	}
}