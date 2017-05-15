using DotnetSpider.Core.Selector;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DotnetSpider.Core.Test
{
	[TestClass]
	public class HtmlTest
	{
		[TestMethod]
		public void Select()
		{
			Selectable selectable = new Selectable("aaaaaaab", "", ContentType.Html);
			string value = selectable.Regex("(.*)").GetValue();
			Assert.AreEqual("aaaaaaab", value);
		}

		[TestMethod]
		public void DonotDetectDomain()
		{
			Selectable selectable = new Selectable("<div><a href=\"www.aaaa.com\">aaaaaaab</a></div>", "", ContentType.Html);
			var values = selectable.XPath(".//a").GetValues();
			Assert.AreEqual("aaaaaaab", values[0]);
		}

		[TestMethod]
		public void DetectDomain1()
		{
			Selectable selectable = new Selectable("<div><a href=\"www.aaaa.com\">aaaaaaab</a></div>", "", ContentType.Html, "www\\.aaaa\\.com");
			var values = selectable.XPath(".//a").GetValues();
			Assert.AreEqual("aaaaaaab", values[0]);
		}

		[TestMethod]
		public void DetectDomain2()
		{
			Selectable selectable = new Selectable("<div><a href=\"www.aaaab.com\">aaaaaaab</a></div>", "", ContentType.Html, "www\\.aaaa\\.com");
			var values = selectable.XPath(".//a").GetValues();
			Assert.AreEqual(0, values.Count);
		}
	}
}
