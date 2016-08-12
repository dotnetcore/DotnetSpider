using System;
using System.Security.Authentication;
using DotnetSpider.Core.Selector;
using Xunit;

namespace DotnetSpider.Test.Selector
{

	public class RegexSelectorTest
	{
#if !NET_CORE
		[Fact]
		public void TestRegexWithSingleLeftBracket()
		{

			Exception ex = Assert.Throws<ArgumentException>(() => new RegexSelector("\\d+("));

			Assert.NotNull(ex);
		}
#endif

		[Fact]
		public void TestRegexWithLeftBracketQuoted()
		{
			string regex = "\\(.+";
			string source = "(hello world";
			RegexSelector regexSelector = new RegexSelector(regex);
			string select = regexSelector.Select(source);
			Assert.Equal(select, source);
		}
	}
}
