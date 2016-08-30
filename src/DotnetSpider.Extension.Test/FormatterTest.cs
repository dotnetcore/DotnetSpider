using System;
using DotnetSpider.Core;
using Xunit;
using DotnetSpider.Extension.Model.Formatter;

namespace DotnetSpider.Extension.Test
{
	public class FormatterTest
	{
		[Fact]
		public void BoolFormatterTest()
		{
			try
			{
				var f = new BoolFormatter { False = "F", ValueWhenNull = "Y", Pattern = "  ", True = "Y" };
				f.Formate("");
				throw new Exception("TEST FAILED.");
			}
			catch (SpiderException se)
			{
				Assert.Equal("Pattern should not be null.", se.Message);
			}
			try
			{
				var f = new BoolFormatter { False = "F", ValueWhenNull = "Y", Pattern = "", True = "Y" };
				f.Formate("");
				throw new Exception("TEST FAILED.");
			}
			catch (SpiderException se)
			{
				Assert.Equal("Pattern should not be null.", se.Message);
			}
			try
			{
				var f = new BoolFormatter { False = "F", ValueWhenNull = "Y", Pattern = null, True = "Y" };
				f.Formate("");
				throw new Exception("TEST FAILED.");
			}
			catch (SpiderException se)
			{
				Assert.Equal("Pattern should not be null.", se.Message);
			}

			BoolFormatter boolFormatter1 = new BoolFormatter { False = "F", ValueWhenNull = "Y", Pattern = "很抱歉", True = "Y" };
			string str1 = "";
			Assert.Equal("Y", boolFormatter1.Formate(str1));

			BoolFormatter boolFormatter2 = new BoolFormatter { False = "F", ValueWhenNull = "F", Pattern = "很抱歉", True = "Y" };
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

		[Fact]
		public void RegexMapValueFormatterTest()
		{
			try
			{
				var f = new RegexMapValueFormatter { Patterns = null };
				f.Formate("");
				throw new Exception("TEST FAILED.");
			}
			catch (SpiderException se)
			{
				Assert.Equal("Arguments incorrect. Patterns and Values should not be null, and count should be same.", se.Message);
			}

			try
			{
				var f = new RegexMapValueFormatter { Patterns = new string[] { } };
				f.Formate("");
				throw new Exception("TEST FAILED.");
			}
			catch (SpiderException se)
			{
				Assert.Equal("Arguments incorrect. Patterns and Values should not be null, and count should be same.", se.Message);
			}
			try
			{
				var f = new RegexMapValueFormatter { Patterns = new[] { "abc" }, Values = null };
				f.Formate("");
				throw new Exception("TEST FAILED.");
			}
			catch (SpiderException se)
			{
				Assert.Equal("Arguments incorrect. Patterns and Values should not be null, and count should be same.", se.Message);
			}
			try
			{
				var f = new RegexMapValueFormatter { Patterns = new[] { "abc" }, Values = new string[] { } };
				f.Formate("");
				throw new Exception("TEST FAILED.");
			}
			catch (SpiderException se)
			{
				Assert.Equal("Arguments incorrect. Patterns and Values should not be null, and count should be same.", se.Message);
			}
			try
			{
				var f = new RegexMapValueFormatter { Patterns = new[] { "abc" }, Values = new[] { "t", "tt" } };
				f.Formate("");
				throw new Exception("TEST FAILED.");
			}
			catch (SpiderException se)
			{
				Assert.Equal("Arguments incorrect. Patterns and Values should not be null, and count should be same.", se.Message);
			}

			RegexMapValueFormatter formatter1 = new RegexMapValueFormatter { Patterns = new[] { "abd" }, Values = new[] { "dba" } };
			string str1 = "cabdeeee";
			Assert.Equal("dba", formatter1.Formate(str1));
			string str2 = "cbeeee";
			Assert.Equal(string.Empty, formatter1.Formate(str2));

			RegexMapValueFormatter formatter2 = new RegexMapValueFormatter { Patterns = new[] { "abd", "CCD" }, Values = new[] { "dba", "xxb" } };
			Assert.Equal("dba", formatter2.Formate(str1));
			Assert.Equal(string.Empty, formatter2.Formate(str2));

			string str3 = "abdCCD";
			Assert.Equal("dba", formatter2.Formate(str3));
			string str4 = "CCDabd";
			Assert.Equal("dba", formatter2.Formate(str4));
			string str5 = "CCDabc";
			Assert.Equal("xxb", formatter2.Formate(str5));
		}
	}
}
