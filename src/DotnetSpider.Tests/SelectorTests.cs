using System;
using System.Linq;
using System.Text.RegularExpressions;
using DotnetSpider.Infrastructure;
using DotnetSpider.Selector;
using Xunit;

namespace DotnetSpider.Tests
{
	public class SelectorTests
	{
		private readonly string html = "<div><h1>test<a href=\"xxx\">aabbcc</a></h1></div>";

		private readonly string html2 =
			"<div><a href='http://whatever.com/aaa'></a></div><div><a href='http://whatever.com/bbb'></a></div>";

		private readonly string json = "{ \"store\": {\n\n\n" +
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

		[Fact(DisplayName = "RemoveOutboundLinks")]
		public void RemoveOutboundLinks()
		{
			// 绝对路径不需要做补充
			var selectable2 = new HtmlSelectable("<div><a href=\"http://www.aaaa.com\">aaaaaaab</a></div>",
				"http://www.b.com");
			var value2 = selectable2.SelectList(Selectors.XPath(".//a"));
			Assert.Null(value2);
		}

		[Fact(DisplayName = "DoNotFixAllRelativeHrefs")]
		public void DoNotFixAllRelativeHrefs()
		{
			var selectable = new HtmlSelectable("<div><a href=\"aaaa.com\">aaaaaaab</a></div>");
			var values = selectable.SelectList(Selectors.XPath(".//a")).ToArray();
			Assert.Equal("aaaaaaab", values.First().Value);
		}

		[Fact(DisplayName = "FixRelativeUrl")]
		public void FixRelativeUrl()
		{
			var absoluteUrl =
				UriUtilities.CanonicalizeUrl("?aa", "http://www.dianping.com/sh/ss/com");
			Assert.Equal("http://www.dianping.com/sh/ss/com?aa", absoluteUrl);

			absoluteUrl =
				UriUtilities.CanonicalizeUrl("../aa", "http://www.dianping.com/sh/ss/com");
			Assert.Equal("http://www.dianping.com/sh/aa", absoluteUrl);

			absoluteUrl = UriUtilities.CanonicalizeUrl("..aa", "http://www.dianping.com/sh/ss/com");
			Assert.Equal("http://www.dianping.com/sh/ss/..aa", absoluteUrl);

			absoluteUrl =
				UriUtilities.CanonicalizeUrl("../../aa", "http://www.dianping.com/sh/ss/com/");
			Assert.Equal("http://www.dianping.com/sh/aa", absoluteUrl);

			absoluteUrl =
				UriUtilities.CanonicalizeUrl("../../aa", "http://www.dianping.com/sh/ss/com");
			Assert.Equal("http://www.dianping.com/aa", absoluteUrl);

			// 只有相对路径需要做补充
			var selectable1 = new HtmlSelectable("<div><a href=\"/a/b\">aaaaaaab</a></div>", "http://www.b.com");
			var value1 = selectable1.Links().First();
			Assert.Equal("http://www.b.com/a/b", value1);

			// 绝对路径不需要做补充
			var selectable2 = new HtmlSelectable("<div><a href=\"http://www.aaaa.com\">aaaaaaab</a></div>",
				"http://www.b.com", false);
			var value2 = selectable2.SelectList(Selectors.XPath(".//a")).First().Value;
			Assert.Equal("aaaaaaab", value2);
		}

		[Fact(DisplayName = "RegexSelector")]
		public void Regex()
		{
			Assert.Equal("a href=\"xxx\"", Selectors.Regex("a href=\"(.*)\"").Select(html).Value);
			Assert.Equal("xxx", Selectors.Regex("(a href)=\"(.*)\"", RegexOptions.None, "$2").Select(html).Value);
		}


		[Fact(DisplayName = "CssSelector")]
		public void Css()
		{
			var result = Selectors.Css("div h1 a").Select(html);
			Assert.Equal("aabbcc", result.Value);
			Assert.Equal("xxx", Selectors.Css("div h1 a", "href").Select(html).Value);
			Assert.Equal("aabbcc", Selectors.Css("div h1 a").Select(html).Value);
		}

		[Fact(DisplayName = "XpathSelector")]
		public void Xpath()
		{
			Assert.Equal("xxx", Selectors.XPath("//a/@href").Select(html).Value);
		}

		[Fact(DisplayName = "XpathBySelectable")]
		public void XpathBySelectable()
		{
			var selectable = new TextSelectable("aaaaaaab");
			var value = selectable.Regex("(.*)").Value;
			Assert.Equal("aaaaaaab", value);
		}

		[Fact(DisplayName = "JsonPathSelector")]
		public void JsonPath()
		{
			var jsonPathSelector = new JsonPathSelector("$.store.book[*].author");
			var result1 = jsonPathSelector.SelectList(json);
			var list1 = jsonPathSelector.SelectList(json).Select(x => x.Value).ToList();
			Assert.Equal("Nigel Rees", result1.First().Value);
			Assert.Contains("Nigel Rees", list1);
			Assert.Contains("Evelyn Waugh", list1);

			jsonPathSelector = new JsonPathSelector("$.store.book[?(@.category == 'reference')]");
			var list2 = jsonPathSelector.SelectList(json);
			var result2 = jsonPathSelector.Select(json).Value;

			var expected1 =
				$"{{{Environment.NewLine}  \"category\": \"reference\",{Environment.NewLine}  \"author\": \"Nigel Rees\",{Environment.NewLine}  \"title\": \"Sayings of the Century\",{Environment.NewLine}  \"price\": 8.95{Environment.NewLine}}}";
			var expected2 =
				$"{{{Environment.NewLine}  \"category\": \"reference\",{Environment.NewLine}  \"author\": \"Nigel Rees\",{Environment.NewLine}  \"title\": \"Sayings of the Century\",{Environment.NewLine}  \"price\": 8.95{Environment.NewLine}}}";
			Assert.Equal(result2, expected1);
			Assert.Equal(list2.First().Value, expected2);
		}

		[Fact(DisplayName = "RegexSelectorException")]
		public void RegexException()
		{
			try
			{
				var selector = new RegexSelector("\\d+(");
				selector.Select("");

				throw new Exception("Test case failed");
			}
			catch (Exception e)
			{
				Assert.Equal("Invalid pattern '\\d+(' at offset 4. Not enough )'s.", e.Message);
			}
		}


		[Fact(DisplayName = "RegexSelectorWithLeftBracketQuoted")]
		public void TestRegexWithLeftBracketQuoted()
		{
			var regex = "\\(.+";
			var source = "(hello world";
			var regexSelector = new RegexSelector(regex);
			string select = regexSelector.Select(source).Value;
			Assert.Equal(select, source);
		}

		[Fact(DisplayName = "XPathSelector2")]
		public void XPath2()
		{
			var selectable = new HtmlSelectable(html2);
			var links1 = selectable.Links();
			var divs = selectable.SelectList(Selectors.XPath("//div")).ToList();
			var link2 = divs[0].Links().ToList();
			var link3 = divs[1].Links().ToList();
			Assert.Equal(2, links1.Count());
			Assert.Single(link2);
			Assert.Single(link3);
			Assert.Equal("http://whatever.com/aaa", link2[0]);
			Assert.Equal("http://whatever.com/bbb", link3[0]);
		}

		[Fact(DisplayName = "Selectable")]
		public void Selectable()
		{
			var selectable = new HtmlSelectable(html2);
			var links = selectable.XPath(".//a/@href").Nodes();
			Assert.Equal("http://whatever.com/aaa", links.First().Value);

			var links1 = selectable.XPath(".//a/@href").Value;
			Assert.Equal("http://whatever.com/aaa", links1);
		}

		[Fact(DisplayName = "PseudoFirst")]
		public void PseudoFirstTest()
		{
			var text =
				@"<ul>
<li class=""top""><span class=""date"" style=""display: block;"">x</span><span class=""title""><a target=""_blank"" href=""https://www.aaa.com/html/it/343752.htm"">aaaa</a></span></li>
<li class=""new""><span class=""date"" style=""display: block;"">y</span><span class=""title""><a target=""_blank"" href=""https://www.aaa.com/html/digi/346221.htm"">bbbb</a></span></li>
<li class=""new""><span class=""date"" style=""display: block;"">z</span><span class=""title""><a target=""_blank"" href=""https://www.aaa.com/html/it/346264.htm"">cccc</a></span></li></ul>";

			ISelectable selectable = new HtmlSelectable(text);
			var result1 = selectable.Select(new CssSelector("ul li a")).Value;
			Assert.Equal("aaaa", result1);
			//var result2 = selectable.Select(new CssSelector("ul li a")).GetValue();
			//Assert.Equal("cccc", result2);
		}
	}
}
