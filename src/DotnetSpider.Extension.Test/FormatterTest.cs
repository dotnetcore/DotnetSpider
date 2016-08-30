using System;
using DotnetSpider.Core;
using Xunit;
using DotnetSpider.Extension.Model.Formatter;

namespace DotnetSpider.Extension.Test
{
	public class FormatterTest
	{
		[Fact]
		public void RegexFormatterTest()
		{
			try
			{
				var f = new RegexFormatter { False = "F", ValueWhenNull = "Y", Pattern = "  ", True = "Y" };
				f.Formate("");
				throw new Exception("TEST FAILED.");
			}
			catch (SpiderException se)
			{
				Assert.Equal("Pattern should not be null or empty.", se.Message);
			}
			try
			{
				var f = new RegexFormatter { False = "F", ValueWhenNull = "Y", Pattern = "", True = "Y" };
				f.Formate("");
				throw new Exception("TEST FAILED.");
			}
			catch (SpiderException se)
			{
				Assert.Equal("Pattern should not be null or empty.", se.Message);
			}
			try
			{
				var f = new RegexFormatter { False = "F", ValueWhenNull = "Y", Pattern = null, True = "Y" };
				f.Formate("");
				throw new Exception("TEST FAILED.");
			}
			catch (SpiderException se)
			{
				Assert.Equal("Pattern should not be null or empty.", se.Message);
			}

			RegexFormatter boolFormatter1 = new RegexFormatter { False = "F", ValueWhenNull = "Y", Pattern = "很抱歉", True = "Y" };
			string str1 = "";
			Assert.Equal("Y", boolFormatter1.Formate(str1));

			RegexFormatter boolFormatter2 = new RegexFormatter { False = "F", ValueWhenNull = "F", Pattern = "很抱歉", True = "Y" };
			Assert.Equal("F", boolFormatter2.Formate(str1));

			string str2 = "ABCD";
			Assert.Equal("F", boolFormatter2.Formate(str2));
			Assert.Equal("F", boolFormatter1.Formate(str2));

			string str3 = "ABCD很抱歉";
			Assert.Equal("Y", boolFormatter2.Formate(str3));
			Assert.Equal("Y", boolFormatter1.Formate(str3));

			string str4 = "很抱歉ABCD";
			Assert.Equal("Y", boolFormatter2.Formate(str4));
			Assert.Equal("Y", boolFormatter1.Formate(str4));

			string str5 = "ABCD很抱歉ABCD";
			Assert.Equal("Y", boolFormatter2.Formate(str5));
			Assert.Equal("Y", boolFormatter1.Formate(str5));

			string str6 = "       很抱歉ABCD";
			Assert.Equal("Y", boolFormatter2.Formate(str6));
			Assert.Equal("Y", boolFormatter1.Formate(str6));
		}
	}
}
