using System.Collections.Generic;
using DotnetSpider.Core.Selector;
using Xunit;
namespace DotnetSpider.Test.Selector
{
	
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

		[Fact]
		public void TestJsonPath()
		{
			JsonPathSelector jsonPathSelector = new JsonPathSelector("$.store.book[*].author");
			string select = jsonPathSelector.Select(_text).ToString();
			IList<dynamic> list = jsonPathSelector.SelectList(_text);
			Assert.Equal(select, "Nigel Rees");
			Assert.True(list.Contains("Nigel Rees"));
			Assert.True(list.Contains("Evelyn Waugh"));

			jsonPathSelector = new JsonPathSelector("$.store.book[?(@.category == 'reference')]");
			jsonPathSelector.SelectList(_text);
			jsonPathSelector.Select(_text);

			//			{
			//				"category": "reference",
			//  "author": "Nigel Rees",
			//  "title": "Sayings of the Century",
			//  "price": 8.95
			//}
			//Assert.Equal(select, "{\r\n \"category\":\"reference\",\r\n \"author\":\"Nigel Rees\",\r\n \"title\":\"Sayings of the Century\",\r\n \"price\":8.95\r\n }");
			//Assert.Equal(list, "{\"author\":\"Nigel Rees\",\"title\":\"Sayings of the Century\",\"category\":\"reference\",\"price\":8.95}");
		}
	}
}
