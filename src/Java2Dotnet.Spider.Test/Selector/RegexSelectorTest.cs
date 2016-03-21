using System;
using Java2Dotnet.Spider.Core.Selector;
#if !NET_CORE
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace Java2Dotnet.Spider.Test.Selector
{
	[TestClass]
	public class RegexSelectorTest
	{
#if !NET_CORE
		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void TestRegexWithSingleLeftBracket()
		{
			string regex = "\\d+(";
			new RegexSelector(regex);
		}
#endif

		[TestMethod]
		public void TestRegexWithLeftBracketQuoted()
		{
			string regex = "\\(.+";
			string source = "(hello world";
			RegexSelector regexSelector = new RegexSelector(regex);
			string select = regexSelector.Select(source);
			Assert.AreEqual(select, source);
		}
	}
}
