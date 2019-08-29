using System;
using System.Linq;
using DotnetSpider.Selector;
using Xunit;

namespace DotnetSpider.Tests.Selector
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
            var selectable2 = new Selectable("<div><a href=\"http://www.aaaa.com\">aaaaaaab</a></div>",
                "http://www.b.com");
            var value2 = selectable2.XPath(".//a").GetValues();
            Assert.Empty(value2);
        }

        [Fact(DisplayName = "DoNotFixAllRelativeHrefs")]
        public void DoNotFixAllRelativeHrefs()
        {
            var selectable = new Selectable("<div><a href=\"aaaa.com\">aaaaaaab</a></div>", null);
            var values = selectable.XPath(".//a").GetValues();
            Assert.Equal("aaaaaaab", values.First());
        }

        [Fact(DisplayName = "FixRelativeUrl")]
        public void FixRelativeUrl()
        {
            var absoluteUrl =
                DotnetSpider.Selector.Selectable.CanonicalizeUrl("?aa", "http://www.dianping.com/sh/ss/com");
            Assert.Equal("http://www.dianping.com/sh/ss/com?aa", absoluteUrl);

            absoluteUrl =
                DotnetSpider.Selector.Selectable.CanonicalizeUrl("../aa", "http://www.dianping.com/sh/ss/com");
            Assert.Equal("http://www.dianping.com/sh/aa", absoluteUrl);

            absoluteUrl = DotnetSpider.Selector.Selectable.CanonicalizeUrl("..aa", "http://www.dianping.com/sh/ss/com");
            Assert.Equal("http://www.dianping.com/sh/ss/..aa", absoluteUrl);

            absoluteUrl =
                DotnetSpider.Selector.Selectable.CanonicalizeUrl("../../aa", "http://www.dianping.com/sh/ss/com/");
            Assert.Equal("http://www.dianping.com/sh/aa", absoluteUrl);

            absoluteUrl =
                DotnetSpider.Selector.Selectable.CanonicalizeUrl("../../aa", "http://www.dianping.com/sh/ss/com");
            Assert.Equal("http://www.dianping.com/aa", absoluteUrl);

            // 只有相对路径需要做补充
            var selectable1 = new Selectable("<div><a href=\"/a/b\">aaaaaaab</a></div>", "http://www.b.com");
            var value1 = selectable1.XPath(".//a").Links().GetValue();
            Assert.Equal("http://www.b.com/a/b", value1);

            // 绝对路径不需要做补充
            var selectable2 = new Selectable("<div><a href=\"http://www.aaaa.com\">aaaaaaab</a></div>",
                "http://www.b.com", false);
            var value2 = selectable2.XPath(".//a").GetValues().First();
            Assert.Equal("aaaaaaab", value2);
        }

        [Fact(DisplayName = "RegexSelector")]
        public void Regex()
        {
            Assert.Equal(Selectors.Regex("a href=\"(.*)\"").Select(html), "a href=\"xxx\"");
            Assert.Equal(Selectors.Regex("(a href)=\"(.*)\"", 2).Select(html), "xxx");
        }


        [Fact(DisplayName = "CssSelector")]
        public void Css()
        {
            Assert.Equal(Selectors.Css("div h1 a").Select(html).OuterHtml, "<a href=\"xxx\">aabbcc</a>");
            Assert.Equal(Selectors.Css("div h1 a", "href").Select(html), "xxx");
            Assert.Equal(Selectors.Css("div h1 a").Select(html).InnerHtml, "aabbcc");
        }

        [Fact(DisplayName = "XpathSelector")]
        public void Xpath()
        {
            Assert.Equal(Selectors.XPath("//a/@href").Select(html), "xxx");
        }

        [Fact(DisplayName = "XpathBySelectable")]
        public void XpathBySelectable()
        {
            var selectable = new Selectable("aaaaaaab");
            var value = selectable.Regex("(.*)").GetValue();
            Assert.Equal("aaaaaaab", value);
        }

        [Fact(DisplayName = "JsonPathSelector")]
        public void JsonPath()
        {
            var jsonPathSelector = new JsonPathSelector("$.store.book[*].author");
            var result1 = jsonPathSelector.Select(json).ToString();
            var list1 = jsonPathSelector.SelectList(json).ToList();
            Assert.Equal(result1, "Nigel Rees");
            Assert.Contains("Nigel Rees", list1);
            Assert.Contains("Evelyn Waugh", list1);

            jsonPathSelector = new JsonPathSelector("$.store.book[?(@.category == 'reference')]");
            var list2 = jsonPathSelector.SelectList(json);
            var result2 = jsonPathSelector.Select(json);

            var expected1 =
                $"{{{Environment.NewLine}  \"category\": \"reference\",{Environment.NewLine}  \"author\": \"Nigel Rees\",{Environment.NewLine}  \"title\": \"Sayings of the Century\",{Environment.NewLine}  \"price\": 8.95{Environment.NewLine}}}";
            var expected2 =
                $"{{{Environment.NewLine}  \"category\": \"reference\",{Environment.NewLine}  \"author\": \"Nigel Rees\",{Environment.NewLine}  \"title\": \"Sayings of the Century\",{Environment.NewLine}  \"price\": 8.95{Environment.NewLine}}}";
            Assert.Equal(result2, expected1);
            Assert.Equal(list2.First(), expected2);
        }

        [Fact(DisplayName = "RegexSelectorException")]
        public void RegexException()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                var selector = new RegexSelector("\\d+(");
                selector.Select("");
            });
        }


        [Fact(DisplayName = "RegexSelectorWithLeftBracketQuoted")]
        public void TestRegexWithLeftBracketQuoted()
        {
            var regex = "\\(.+";
            var source = "(hello world";
            var regexSelector = new RegexSelector(regex);
            string select = regexSelector.Select(source);
            Assert.Equal(select, source);
        }

        [Fact(DisplayName = "XPathSelector2")]
        public void XPath2()
        {
            var selectable = new Selectable(html2);
            var linksWithoutChain = selectable.Links().GetValues();
            var xpath = selectable.XPath("//div");
            var linksWithChainFirstCall = xpath.Links().GetValues().ToList();
            var linksWithChainSecondCall = xpath.Links().GetValues().ToList();
            Assert.Equal(linksWithoutChain.Count(), linksWithChainFirstCall.Count());
            Assert.Equal(linksWithChainFirstCall.Count(), linksWithChainSecondCall.Count());
        }

        [Fact(DisplayName = "Selectable")]
        public void Selectable()
        {
            var selectable = new Selectable(html2);
            var links = selectable.XPath(".//a/@href").Nodes();
            Assert.Equal("http://whatever.com/aaa", links.First().GetValue());

            var links1 = selectable.XPath(".//a/@href").GetValue();
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

            ISelectable selectable = new Selectable(text);
            var result1 = selectable.Select(new CssSelector("ul li a")).GetValue();
            Assert.Equal("aaaa", result1);
            //var result2 = selectable.Select(new CssSelector("ul li a")).GetValue();
            //Assert.Equal("cccc", result2);
        }
    }
}