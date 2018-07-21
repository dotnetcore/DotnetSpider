using DotnetSpider.Extraction.Model;
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
				Extension.Infrastructure.SelectorUtil.NotNullExpression(new Selector(""));
			});
			Assert.Throws<ArgumentException>(() =>
			{
				Extension.Infrastructure.SelectorUtil.NotNullExpression(new Selector(null));
			});
			Assert.Throws<ArgumentException>(() =>
			{
				Extension.Infrastructure.SelectorUtil.NotNullExpression(new Selector("  "));
			});
		}
	}
}
