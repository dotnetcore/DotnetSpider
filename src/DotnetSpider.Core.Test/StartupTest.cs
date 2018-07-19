using System;
using Xunit;
using System.Linq;

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
		[Fact(DisplayName = "AnalyzeUnCorrectArguments")]
		public void AnalyzeUnCorrectArguments()
		{
			var args1 = new[] { "-s:DotnetSpider.Core.Test.TestSpider", "--tid:TestSpider", "-i:guid", "-a:", "abcd" };
			var arguments1 = Startup.Parse(args1).GetArguments();
			Assert.Empty(arguments1);

			var args2 = new[] { "-s:DotnetSpider.Core.Test.TestSpider", "--tid:TestSpider", "-i:guid", "-a::::" };
			var arguments2 = Startup.Parse(args2).GetArguments();
			Assert.Empty(arguments2);

			var args3 = new[] { "-ti:DotnetSpider.Core.Test.TestSpider" };
			var arguments3 = Startup.Parse(args3);
			Assert.Null(arguments3);

			var args4 = new[] { "-s:DotnetSpider.Core.Test.TestSpider" };
			var arguments4 = Startup.Parse(args4).GetArguments();
			Assert.Empty(arguments4);
		}

		[Fact(DisplayName = "AnalyzeArguments")]
		public void AnalyzeArguments()
		{
			var args = new[] { "-s:DotnetSpider.Core.Test.TestSpider", "--tid:TestSpider", "-i:guid", "-a:a,b", "-n:myname" };
			var arguments = Startup.Parse(args);
			Assert.Equal("DotnetSpider.Core.Test.TestSpider", arguments.Spider);
			Assert.Equal("TestSpider", arguments.TaskId);
			Assert.Equal("guid", arguments.Identity);
			Assert.Equal("myname", arguments.Name);
			Assert.Equal("a", arguments.GetArguments().ElementAt(0));
			Assert.Equal("b", arguments.GetArguments().ElementAt(1));

			var args2 = new[] { "-s:DotnetSpider.Core.Test.TestSpider", "--tid:TestSpider", "-i:guid", "-a:    asdf" };
			var arguments2 = Startup.Parse(args2);
			Assert.Equal("asdf", arguments2.GetArguments().ElementAt(0));

			var args3 = new[] { "-s:DotnetSpider.Core.Test.TestSpider", "--tid:TestSpider", "-i:guid" };
			var arguments3 = Startup.Parse(args3);
			Assert.Equal("TestSpider", arguments3.TaskId);
		}

		[Fact(DisplayName = "DetectCorrectSpiderCount")]
		public void DetectCorrectSpiderCount()
		{
			var args = new[] { "-s:DotnetSpider.Core.Test.TestSpider", "--tid:TestSpider", "-i:guid", "-a:" };
			var arguments = Startup.Parse(args);
			var spiderTypes = Startup.DetectSpiders();
			Assert.Single(spiderTypes);
		}

		[Fact(DisplayName = "SetGuidIdentity")]
		public void SetGuidIdentity()
		{
			var args = new[] { "-s:DotnetSpider.Core.Test.TestSpider", "--tid:TestSpider", "-i:guid", "-a:" };
			var arguments = Startup.Parse(args);
			var spiderTypes = Startup.DetectSpiders();
			var spider = (TestSpider)Startup.CreateSpiderInstance("DotnetSpider.Core.Test.TestSpider", arguments, spiderTypes);
			Guid.Parse(spider.Identity);
		}

		[Fact(DisplayName = "SetIdentity")]
		public void SetIdentity()
		{
			var args = new[] { "-s:DotnetSpider.Core.Test.TestSpider", "--tid:TestSpider", "-i:WHAT", "-a:" };
			var arguments = Startup.Parse(args);
			var spiderTypes = Startup.DetectSpiders();
			var spider = (TestSpider)Startup.CreateSpiderInstance("DotnetSpider.Core.Test.TestSpider", arguments, spiderTypes);
			Assert.Equal("WHAT", spider.Identity);
		}

		[Fact(DisplayName = "SetTaskId")]
		public void SetTaskId()
		{
			var args = new[] { "-s:DotnetSpider.Core.Test.TestSpider", "--tid:TestSpider", "-i:guid", "-a:" };
			var arguments = Startup.Parse(args);
			var spiderTypes = Startup.DetectSpiders();
			var spider = (TestSpider)Startup.CreateSpiderInstance("DotnetSpider.Core.Test.TestSpider", arguments, spiderTypes);
			Assert.Equal("TestSpider", spider.TaskId);
		}

		[Fact(DisplayName = "SetSpiderName")]
		public void SetSpiderName()
		{
			var args = new[] { "-s:DotnetSpider.Core.Test.TestSpider", "--tid:TestSpider", "-n:What", "-i:guid", "-a:" };
			var arguments = Startup.Parse(args);
			var spiderTypes = Startup.DetectSpiders();
			var spider = (TestSpider)Startup.CreateSpiderInstance("DotnetSpider.Core.Test.TestSpider", arguments, spiderTypes);
			Assert.Equal("What", spider.Name);
		}
	}
}
