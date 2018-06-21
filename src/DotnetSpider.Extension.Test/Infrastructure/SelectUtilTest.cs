using DotnetSpider.Extension.Infrastructure;
using DotnetSpider.Extension.Model;
using System;
using Xunit;

namespace DotnetSpider.Extension.Test.Infrastructure
{
	public class SelectUtilTest
	{
		[Fact(DisplayName = "SelectUtil_NotNullExpression")]
		public void NotNullExpression()
		{
			Assert.Throws<ArgumentException>(() =>
			{
				SelectorUtil.NotNullExpression(new Selector(""));
			});
			Assert.Throws<ArgumentException>(() =>
			{
				SelectorUtil.NotNullExpression(new Selector(null));
			});
			Assert.Throws<ArgumentException>(() =>
			{
				SelectorUtil.NotNullExpression(new Selector("  "));
			});
		}
	}
}
