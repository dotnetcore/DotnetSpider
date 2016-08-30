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

			RegexFormatter formatter1 = new RegexFormatter { False = "F", ValueWhenNull = "Y", Pattern = "很抱歉", True = "Y" };
			string str1 = "";
			Assert.Equal("Y", formatter1.Formate(str1));

			RegexFormatter formatter2 = new RegexFormatter { False = "F", ValueWhenNull = "F", Pattern = "很抱歉", True = "Y" };
			Assert.Equal("F", formatter2.Formate(str1));

			string str2 = "ABCD";
			Assert.Equal("F", formatter2.Formate(str2));
			Assert.Equal("F", formatter1.Formate(str2));

			string str3 = "ABCD很抱歉";
			Assert.Equal("Y", formatter2.Formate(str3));
			Assert.Equal("Y", formatter1.Formate(str3));

			string str4 = "很抱歉ABCD";
			Assert.Equal("Y", formatter2.Formate(str4));
			Assert.Equal("Y", formatter1.Formate(str4));

			string str5 = "ABCD很抱歉ABCD";
			Assert.Equal("Y", formatter2.Formate(str5));
			Assert.Equal("Y", formatter1.Formate(str5));

			string str6 = "       很抱歉ABCD";
			Assert.Equal("Y", formatter2.Formate(str6));
			Assert.Equal("Y", formatter1.Formate(str6));

			RegexFormatter formatter3 = new RegexFormatter { Pattern = "很抱歉", True = "Y" };
			Assert.Equal(str2, formatter3.Formate(str2));
			Assert.Equal("Y", formatter3.Formate(str3));

			RegexFormatter formatter4 = new RegexFormatter { Pattern = "很抱歉", False = "N" };
			Assert.Equal("N", formatter4.Formate(str2));
			Assert.Equal("很抱歉", formatter4.Formate(str3));

			string str7 = "收货100人啊啊";
			RegexFormatter formatter5 = new RegexFormatter { Pattern = @"收货[\d]+人" };
			Assert.Equal("ABCD", formatter5.Formate(str2));
			Assert.Equal("ABCD很抱歉", formatter5.Formate(str3));
			Assert.Equal("收货100人", formatter5.Formate(str7));
		}

		[Fact]
		public void CharacterCaseFormatterTest()
		{
			CharacterCaseFormatter formatter1 = new CharacterCaseFormatter();
			string str1 = "";
			Assert.Equal("", formatter1.Formate(str1));
			Assert.Equal(string.Empty, formatter1.Formate(null));

			string str2 = "a";
			Assert.Equal("A", formatter1.Formate(str2));

			CharacterCaseFormatter formatter2 = new CharacterCaseFormatter { ToUpper = false };
			Assert.Equal("a", formatter2.Formate(str2));

			string str3 = "A";
			Assert.Equal("a", formatter2.Formate(str3));
			Assert.Equal(string.Empty, formatter2.Formate(null));

			CharacterCaseFormatter formatter3 = new CharacterCaseFormatter { ToUpper = false, ValueWhenNull = "OK" };
			Assert.Equal("OK", formatter3.Formate(null));
			Assert.Equal("OK", formatter3.Formate(""));
		}

		[Fact]
		public void DisplaceFormaterTest()
		{
			DisplaceFormater formatter1 = new DisplaceFormater { Displacement = "d", EqualValue = "a" };
			string str1 = "";
			Assert.Equal("", formatter1.Formate(str1));
			Assert.Equal("d", formatter1.Formate("a"));
			Assert.Equal("dd", formatter1.Formate("dd"));
		}

		[Fact]
		public void FormatStringFormaterTest()
		{
			try
			{
				var f = new FormatStringFormater { Format = "" };
				f.Formate("");
				throw new Exception("TEST FAILED.");
			}
			catch (SpiderException se)
			{
				Assert.Equal("FormatString should not be null or empty.", se.Message);
			}
			try
			{
				var f = new FormatStringFormater { Format = null };
				f.Formate("");
				throw new Exception("TEST FAILED.");
			}
			catch (SpiderException se)
			{
				Assert.Equal("FormatString should not be null or empty.", se.Message);
			}
			try
			{
				var f = new FormatStringFormater { Format = "     " };
				f.Formate("");
				throw new Exception("TEST FAILED.");
			}
			catch (SpiderException se)
			{
				Assert.Equal("FormatString should not be null or empty.", se.Message);
			}

			FormatStringFormater formatter1 = new FormatStringFormater { Format = "http://{0}" };
			Assert.Equal("http://a", formatter1.Formate("a"));

			FormatStringFormater formatter2 = new FormatStringFormater { Format = "http://{0}/{1}" };
			Assert.Equal("http://a/b", formatter2.Formate(new[] { "a", "b" }));
		}
	}
}
