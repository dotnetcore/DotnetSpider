using DotnetSpider.Extension.Infrastructure;
using DotnetSpider.Extension.Model;
using System;
using Xunit;

namespace DotnetSpider.Extension.Test.Infrastructure
{
	public class SelectUtilTest
	{
		[Fact]
		public void NotNullExpression()
		{
			Assert.Throws<ArgumentException>(() =>
			{
				SelectorUtil.NotNullExpression(new SelectorAttribute(""));
			});
			Assert.Throws<ArgumentException>(() =>
			{
				SelectorUtil.NotNullExpression(new SelectorAttribute(null));
			});
			Assert.Throws<ArgumentException>(() =>
			{
				SelectorUtil.NotNullExpression(new SelectorAttribute("  "));
			});
		}
	}
}
