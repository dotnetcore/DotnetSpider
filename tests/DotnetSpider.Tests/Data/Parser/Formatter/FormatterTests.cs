using System;
using DotnetSpider.Data.Parser.Formatter;
using Xunit;

namespace DotnetSpider.Tests.Data.Parser.Formatter
{

	public class FormatterTests
	{
		[Fact(DisplayName = "RegexFormatter")]
		public void RegexFormatterTest()
		{
			try
			{
				var f = new RegexFormatter { False = "F", ValueWhenNull = "Y", Pattern = "  ", True = "Y" };
				f.Format("");
				throw new Exception("TEST FAILED.");
			}
			catch (ArgumentException se)
			{
				Assert.Equal("Pattern should not be null or empty", se.Message);
			}
			try
			{
				var f = new RegexFormatter { False = "F", ValueWhenNull = "Y", Pattern = "", True = "Y" };
				f.Format("");
				throw new Exception("TEST FAILED.");
			}
			catch (ArgumentException se)
			{
				Assert.Equal("Pattern should not be null or empty", se.Message);
			}
			try
			{
				var f = new RegexFormatter { False = "F", ValueWhenNull = "Y", Pattern = null, True = "Y" };
				f.Format("");
				throw new Exception("TEST FAILED.");
			}
			catch (ArgumentException se)
			{
				Assert.Equal("Pattern should not be null or empty", se.Message);
			}

			RegexFormatter formatter1 = new RegexFormatter { False = "F", ValueWhenNull = "Y", Pattern = "很抱歉", True = "Y" };
			string str1 = "";
			Assert.Equal("F", formatter1.Format(str1));

			RegexFormatter formatter2 = new RegexFormatter { False = "F", ValueWhenNull = "F", Pattern = "很抱歉", True = "Y" };
			Assert.Equal("F", formatter2.Format(str1));

			string str2 = "ABCD";
			Assert.Equal("F", formatter2.Format(str2));
			Assert.Equal("F", formatter1.Format(str2));

			string str3 = "ABCD很抱歉";
			Assert.Equal("Y", formatter2.Format(str3));
			Assert.Equal("Y", formatter1.Format(str3));

			string str4 = "很抱歉ABCD";
			Assert.Equal("Y", formatter2.Format(str4));
			Assert.Equal("Y", formatter1.Format(str4));

			string str5 = "ABCD很抱歉ABCD";
			Assert.Equal("Y", formatter2.Format(str5));
			Assert.Equal("Y", formatter1.Format(str5));

			string str6 = "       很抱歉ABCD";
			Assert.Equal("Y", formatter2.Format(str6));
			Assert.Equal("Y", formatter1.Format(str6));

			RegexFormatter formatter3 = new RegexFormatter { Pattern = "很抱歉", True = "Y" };
			Assert.Equal("", formatter3.Format(str2));
			Assert.Equal("Y", formatter3.Format(str3));

			RegexFormatter formatter4 = new RegexFormatter { Pattern = "很抱歉", False = "N" };
			Assert.Equal("N", formatter4.Format(str2));
			Assert.Equal("很抱歉", formatter4.Format(str3));

			string str7 = "收货100人啊啊";
			RegexFormatter formatter5 = new RegexFormatter { Pattern = @"收货[\d]+人" };
			Assert.Equal("", formatter5.Format(str2));
			Assert.Equal("", formatter5.Format(str3));
			Assert.Equal("收货100人", formatter5.Format(str7));
		}

		[Fact(DisplayName = "CharacterCaseFormatter")]
		public void CharacterCaseFormatterTest()
		{
			CharacterCaseFormatter formatter1 = new CharacterCaseFormatter();
			string str1 = "";
			Assert.Equal("", formatter1.Format(str1));
			Assert.Null(formatter1.Format(null));

			string str2 = "a";
			Assert.Equal("A", formatter1.Format(str2));

			CharacterCaseFormatter formatter2 = new CharacterCaseFormatter { ToUpper = false };
			Assert.Equal("a", formatter2.Format(str2));

			string str3 = "A";
			Assert.Equal("a", formatter2.Format(str3));
			Assert.Null(formatter2.Format(null));

			CharacterCaseFormatter formatter3 = new CharacterCaseFormatter { ToUpper = false, ValueWhenNull = "OK" };
			Assert.Equal("OK", formatter3.Format(null));
			Assert.Equal("", formatter3.Format(""));
		}

		[Fact(DisplayName = "DisplaceFormater")]
		public void DisplaceFormaterTest()
		{
			DisplaceFormatter formatter1 = new DisplaceFormatter { Displacement = "d", EqualValue = "a" };
			string str1 = "";
			Assert.Equal("", formatter1.Format(str1));
			Assert.Equal("d", formatter1.Format("a"));
			Assert.Equal("dd", formatter1.Format("dd"));

			//DisplaceFormater formatter2 = new DisplaceFormater { Displacement = 3, EqualValue = 1 };
			//Assert.Equal(2, formatter2.Formate(2));
			//Assert.Equal(3, formatter2.Formate(1));
			//Assert.Equal("dd", formatter2.Formate("dd"));
		}

		[Fact(DisplayName = "FormatStringFormater")]
		public void FormatStringFormaterTest()
		{
			try
			{
				var f = new StringFormatter { FormatStr = "" };
				f.Format("");
				throw new Exception("TEST FAILED.");
			}
			catch (ArgumentException se)
			{
				Assert.Equal("FormatString should not be null or empty", se.Message);
			}
			try
			{
				var f = new StringFormatter { FormatStr = null };
				f.Format("");
				throw new Exception("TEST FAILED.");
			}
			catch (ArgumentException se)
			{
				Assert.Equal("FormatString should not be null or empty", se.Message);
			}
			try
			{
				var f = new StringFormatter { FormatStr = "     " };
				f.Format("");
				throw new Exception("TEST FAILED.");
			}
			catch (ArgumentException se)
			{
				Assert.Equal("FormatString should not be null or empty", se.Message);
			}

			StringFormatter formatter1 = new StringFormatter { FormatStr = "http://{0}" };
			Assert.Equal("http://a", formatter1.Format("a"));

			//StringFormater formatter2 = new StringFormater { Format = "http://{0}/{1}" };
			//Assert.Equal("http://a/b", formatter2.Formate(new[] { "a", "b" }));
		}
	}
}
