using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace DotnetSpider.Core.Test
{
	public class TestSpider : Spider
	{
		public TestSpider()
		{
			Name = "hello";
		}
	}

	public class StartupTest
	{
		[Fact]
		public void AnalyzeUnCorrectArguments()
		{
			var args1 = new[] { "-s:DotnetSpider.Core.Test.TestSpider", "-tid:TestSpider", "-i:guid", "-a:", "abcd" };
			var arguments1 = Startup.AnalyzeArguments(args1);
			Assert.Null(arguments1);

			var args2 = new[] { "-s:DotnetSpider.Core.Test.TestSpider", "-tid:TestSpider", "-i:guid", "-a::::" };
			var arguments2 = Startup.AnalyzeArguments(args2);
			Assert.Null(arguments2);

			var args6 = new[] { "-s:DotnetSpider.Core.Test.TestSpider" };
			var arguments6 = Startup.AnalyzeArguments(args6);
			Assert.Null(arguments6);

			var args3 = new[] { "-ti:DotnetSpider.Core.Test.TestSpider" };
			var arguments3 = Startup.AnalyzeArguments(args3);
			Assert.Null(arguments3);
		}

		[Fact]
		public void AnalyzeArguments()
		{
			var args = new[] { "-s:DotnetSpider.Core.Test.TestSpider", "-tid:TestSpider", "-i:guid", "-a:", "-n:myname" };
			var arguments = Startup.AnalyzeArguments(args);
			Assert.Equal(5, arguments.Count);
			Assert.Equal("DotnetSpider.Core.Test.TestSpider", arguments["-s"]);
			Assert.Equal("TestSpider", arguments["-tid"]);
			Assert.Equal("guid", arguments["-i"]);
			Assert.Equal("myname", arguments["-n"]);
			Assert.Equal("", arguments["-a"]);

			var args3 = new[] { "-s:DotnetSpider.Core.Test.TestSpider", "-tid:TestSpider", "-i:guid", "-a:    asdf" };
			var arguments3 = Startup.AnalyzeArguments(args3);
			Assert.Equal(4, arguments3.Count);
			Assert.Equal("asdf", arguments3["-a"]);

			var args4 = new[] { "-s:DotnetSpider.Core.Test.TestSpider", "-tid:TestSpider", "-i:guid" };
			var arguments4 = Startup.AnalyzeArguments(args4);
			Assert.Equal(3, arguments4.Count);
			Assert.Equal("TestSpider", arguments4["-tid"]);
		}

		[Fact]
		public void DetectCorrectSpiderCount()
		{
			var args = new[] { "-s:DotnetSpider.Core.Test.TestSpider", "-tid:TestSpider", "-i:guid", "-a:" };
			var arguments = Startup.AnalyzeArguments(args);
			var spiderTypes = Startup.DetectSpiders();
			Assert.Equal(2, spiderTypes.Count);
		}

		[Fact]
		public void SetGuidIdentity()
		{
			var args = new[] { "-s:DotnetSpider.Core.Test.TestSpider", "-tid:TestSpider", "-i:guid", "-a:" };
			var arguments = Startup.AnalyzeArguments(args);
			var spiderTypes = Startup.DetectSpiders();
			var spider = (TestSpider)Startup.CreateSpiderInstance("DotnetSpider.Core.Test.TestSpider", arguments, spiderTypes);
			Guid.Parse(spider.Identity);
		}

		[Fact]
		public void SetIdentity()
		{
			var args = new[] { "-s:DotnetSpider.Core.Test.TestSpider", "-tid:TestSpider", "-i:WHAT", "-a:" };
			var arguments = Startup.AnalyzeArguments(args);
			var spiderTypes = Startup.DetectSpiders();
			var spider = (TestSpider)Startup.CreateSpiderInstance("DotnetSpider.Core.Test.TestSpider", arguments, spiderTypes);
			Assert.Equal("WHAT", spider.Identity);
		}

		[Fact]
		public void SetTaskId()
		{
			var args = new[] { "-s:DotnetSpider.Core.Test.TestSpider", "-tid:TestSpider", "-i:guid", "-a:" };
			var arguments = Startup.AnalyzeArguments(args);
			var spiderTypes = Startup.DetectSpiders();
			var spider = (TestSpider)Startup.CreateSpiderInstance("DotnetSpider.Core.Test.TestSpider", arguments, spiderTypes);
			Assert.Equal("TestSpider", spider.TaskId);
		}

		[Fact]
		public void SetSpiderName()
		{
			var args = new[] { "-s:DotnetSpider.Core.Test.TestSpider", "-tid:TestSpider", "-n:What", "-i:guid", "-a:" };
			var arguments = Startup.AnalyzeArguments(args);
			var spiderTypes = Startup.DetectSpiders();
			var spider = (TestSpider)Startup.CreateSpiderInstance("DotnetSpider.Core.Test.TestSpider", arguments, spiderTypes);
			Assert.Equal("What", spider.Name);
		}

		[Fact]
		public void SetReportArgument()
		{
			var args = new[] { "-s:DotnetSpider.Core.Test.TestSpider", "-tid:TestSpider", "-n:What", "-i:guid", "-a:report" };
			var arguments = Startup.AnalyzeArguments(args);
			var spiderTypes = Startup.DetectSpiders();
			var spider = (TestSpider)Startup.CreateSpiderInstance("DotnetSpider.Core.Test.TestSpider", arguments, spiderTypes);
			Assert.Equal(1000, spider.EmptySleepTime);
		}
	}
}
