using System.Collections.Generic;
using Java2Dotnet.Spider.Core.Selector;
#if !NET_CORE
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace Java2Dotnet.Spider.Test.Selector
{
	[TestClass]
	public class JsonPathSelectorTest
	{
		private string _text = "{ \"store\": {\n\n\n" +
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

		[TestMethod]
		public void TestJsonPath()
		{
			JsonPathSelector jsonPathSelector = new JsonPathSelector("$.store.book[*].author");
			string select = jsonPathSelector.Select(_text).ToString();
			IList<dynamic> list = jsonPathSelector.SelectList(_text);
			Assert.AreEqual(select, "Nigel Rees");
			Assert.IsTrue(list.Contains("Nigel Rees"));
			Assert.IsTrue(list.Contains("Evelyn Waugh"));

			jsonPathSelector = new JsonPathSelector("$.store.book[?(@.category == 'reference')]");
			list = jsonPathSelector.SelectList(_text);
			select = jsonPathSelector.Select(_text);

			//			{
			//				"category": "reference",
			//  "author": "Nigel Rees",
			//  "title": "Sayings of the Century",
			//  "price": 8.95
			//}
			//Assert.AreEqual(select, "{\r\n \"category\":\"reference\",\r\n \"author\":\"Nigel Rees\",\r\n \"title\":\"Sayings of the Century\",\r\n \"price\":8.95\r\n }");
			//Assert.AreEqual(list, "{\"author\":\"Nigel Rees\",\"title\":\"Sayings of the Century\",\"category\":\"reference\",\"price\":8.95}");
		}
	}
}
