using System;
using System.Linq;
using Xunit;

namespace DotnetSpider.Extraction.Test
{
	public class SelectorTest
	{
		string _html = "<div><h1>test<a href=\"xxx\">aabbcc</a></h1></div>";

		string _html2 =
			"<div><a href='http://whatever.com/aaa'></a></div><div><a href='http://whatever.com/bbb'></a></div>";

		string _text = "{ \"store\": {\n\n\n" +
					   "    \"book\": [ \n" +
					   "      { \"category\":\n\n\n \"reference\",\n\n\n\n" +
					   "        \"author\": \"Nigel Rees\",\n\n\n\n" +
					   "        \"title\": \"Sayings of the Century\",\n" +
					   "        \"price\": 8.95\n" +
					   "      },\n" +
					   "      { \"category\": \"fiction\",\n" +
					   "        \"author\": \"Evelyn Waugh\",\n" +
					   "        \"title\": \"Sword of Honour\",\n" +
					   "        \"price\": 12.99,\n" +
					   "        \"isbn\": \"0-553-21311-3\"\n" +
					   "      }\n" +
					   "    ],\n" +
					   "    \"bicycle\": {\n" +
					   "      \"color\": \"red\",\n" +
					   "      \"price\": 19.95\n" +
					   "    }\n" +
					   "  }\n" +
					   "}";


		[Fact(DisplayName = "Selector_Regex")]
		public void Regex()
		{
			Assert.Equal(Selectors.Regex("a href=\"(.*)\"").Select(_html), "a href=\"xxx\"");
			Assert.Equal(Selectors.Regex("(a href)=\"(.*)\"", 2).Select(_html), "xxx");
		}



		[Fact(DisplayName = "Selector_Css")]
		public void Css()
		{
			Assert.Equal(Selectors.Css("div h1 a").Select(_html).OuterHtml, "<a href=\"xxx\">aabbcc</a>");
			Assert.Equal(Selectors.Css("div h1 a", "href").Select(_html), "xxx");
			Assert.Equal(Selectors.Css("div h1 a").Select(_html).InnerHtml, "aabbcc");
		}

		[Fact(DisplayName = "Selector_Xpath")]
		public void Xpath()
		{
			Assert.Equal(Selectors.XPath("//a/@href").Select(_html), "xxx");
		}

		[Fact(DisplayName = "Selector_JsonPath")]
		public void JsonPath()
		{
			JsonPathSelector jsonPathSelector = new JsonPathSelector("$.store.book[*].author");
			var result1 = jsonPathSelector.Select(_text).ToString();
			var list1 = jsonPathSelector.SelectList(_text);
			Assert.Equal(result1, "Nigel Rees");
			Assert.Contains("Nigel Rees", list1);
			Assert.Contains("Evelyn Waugh", list1);

			jsonPathSelector = new JsonPathSelector("$.store.book[?(@.category == 'reference')]");
			var list2 = jsonPathSelector.SelectList(_text);
			var result2 = jsonPathSelector.Select(_text);

			var expected1 =
				$"{{{Environment.NewLine}  \"category\": \"reference\",{Environment.NewLine}  \"author\": \"Nigel Rees\",{Environment.NewLine}  \"title\": \"Sayings of the Century\",{Environment.NewLine}  \"price\": 8.95{Environment.NewLine}}}";
			var expected2 =
				$"{{{Environment.NewLine}  \"category\": \"reference\",{Environment.NewLine}  \"author\": \"Nigel Rees\",{Environment.NewLine}  \"title\": \"Sayings of the Century\",{Environment.NewLine}  \"price\": 8.95{Environment.NewLine}}}";
			Assert.Equal(result2, expected1);
			Assert.Equal(list2.First(), expected2);
		}

		[Fact(DisplayName = "Selector_RegexException")]
		public void RegexException()
		{
			Assert.Throws<ArgumentException>(() => { new RegexSelector("\\d+("); });
		}


		[Fact(DisplayName = "Selector_RegexWithLeftBracketQuoted")]
		public void TestRegexWithLeftBracketQuoted()
		{
			string regex = "\\(.+";
			string source = "(hello world";
			RegexSelector regexSelector = new RegexSelector(regex);
			string select = regexSelector.Select(source);
			Assert.Equal(select, source);
		}

		[Fact(DisplayName = "Selector_XPath2")]
		public void XPath2()
		{
			Selectable selectable = new Selectable(_html2);
			var linksWithoutChain = selectable.Links().GetValues();
			ISelectable xpath = selectable.XPath("//div");
			var linksWithChainFirstCall = xpath.Links().GetValues();
			var linksWithChainSecondCall = xpath.Links().GetValues();
			Assert.Equal(linksWithoutChain.Count(), linksWithChainFirstCall.Count());
			Assert.Equal(linksWithChainFirstCall.Count(), linksWithChainSecondCall.Count());
		}

		[Fact(DisplayName = "Selector_Selectable")]
		public void Selectable()
		{
			Selectable selectable = new Selectable(_html2);
			var links = selectable.XPath(".//a/@href").Nodes();
			Assert.Equal("http://whatever.com/aaa", links.First().GetValue());

			var links1 = selectable.XPath(".//a/@href").GetValue();
			Assert.Equal("http://whatever.com/aaa", links1);
		}

		[Fact(DisplayName = "Selector_PseudoFirst")]
		public void PseudoFirstTest()
		{
			var html =
				@"<ul>
<li class=""top""><span class=""date"" style=""display: block;"">x</span><span class=""title""><a target=""_blank"" href=""https://www.aaa.com/html/it/343752.htm"">aaaa</a></span></li>
<li class=""new""><span class=""date"" style=""display: block;"">y</span><span class=""title""><a target=""_blank"" href=""https://www.aaa.com/html/digi/346221.htm"">bbbb</a></span></li>
<li class=""new""><span class=""date"" style=""display: block;"">z</span><span class=""title""><a target=""_blank"" href=""https://www.aaa.com/html/it/346264.htm"">cccc</a></span></li></ul>";

			ISelectable selectable = new Selectable(html);
			var result1 = selectable.Select(new CssSelector("ul li a")).GetValue();
			Assert.Equal("aaaa", result1);
			//var result2 = selectable.Select(new CssSelector("ul li a")).GetValue();
			//Assert.Equal("cccc", result2);
		}
	}
}