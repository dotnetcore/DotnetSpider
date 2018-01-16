using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace DotnetSpider.Core.Test
{
	public class HttpExecuteRecordTest
	{
		[Fact]
		public void Record()
		{
			if (Env.IsWindows)
			{
				Env.EnterpiseServiceIncreaseRunningUrl = "http://localhost:30013/Task/IncreaseRunning";
				Env.EnterpiseServiceReduceRunningUrl = "http://localhost:30013/Task/ReduceRunning";
				var recorder = new HttpExecuteRecord();
				recorder.Add("-1", "test", "abcd");
				recorder.Remove("-1", "test", "abcd");
			}
		}
	}
}
