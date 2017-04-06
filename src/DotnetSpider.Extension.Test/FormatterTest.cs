using System;
using DotnetSpider.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DotnetSpider.Extension.Model.Formatter;

namespace DotnetSpider.Extension.Test
{
	[TestClass]
	public class FormatterTest
	{
		[TestMethod]
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
				Assert.AreEqual("Pattern should not be null or empty.", se.Message);
			}
			try
			{
				var f = new RegexFormatter { False = "F", ValueWhenNull = "Y", Pattern = "", True = "Y" };
				f.Formate("");
				throw new Exception("TEST FAILED.");
			}
			catch (SpiderException se)
			{
				Assert.AreEqual("Pattern should not be null or empty.", se.Message);
			}
			try
			{
				var f = new RegexFormatter { False = "F", ValueWhenNull = "Y", Pattern = null, True = "Y" };
				f.Formate("");
				throw new Exception("TEST FAILED.");
			}
			catch (SpiderException se)
			{
				Assert.AreEqual("Pattern should not be null or empty.", se.Message);
			}

			RegexFormatter formatter1 = new RegexFormatter { False = "F", ValueWhenNull = "Y", Pattern = "很抱歉", True = "Y" };
			string str1 = "";
			Assert.AreEqual("Y", formatter1.Formate(str1));

			RegexFormatter formatter2 = new RegexFormatter { False = "F", ValueWhenNull = "F", Pattern = "很抱歉", True = "Y" };
			Assert.AreEqual("F", formatter2.Formate(str1));

			string str2 = "ABCD";
			Assert.AreEqual("F", formatter2.Formate(str2));
			Assert.AreEqual("F", formatter1.Formate(str2));

			string str3 = "ABCD很抱歉";
			Assert.AreEqual("Y", formatter2.Formate(str3));
			Assert.AreEqual("Y", formatter1.Formate(str3));

			string str4 = "很抱歉ABCD";
			Assert.AreEqual("Y", formatter2.Formate(str4));
			Assert.AreEqual("Y", formatter1.Formate(str4));

			string str5 = "ABCD很抱歉ABCD";
			Assert.AreEqual("Y", formatter2.Formate(str5));
			Assert.AreEqual("Y", formatter1.Formate(str5));

			string str6 = "       很抱歉ABCD";
			Assert.AreEqual("Y", formatter2.Formate(str6));
			Assert.AreEqual("Y", formatter1.Formate(str6));

			RegexFormatter formatter3 = new RegexFormatter { Pattern = "很抱歉", True = "Y" };
			Assert.AreEqual("", formatter3.Formate(str2));
			Assert.AreEqual("Y", formatter3.Formate(str3));

			RegexFormatter formatter4 = new RegexFormatter { Pattern = "很抱歉", False = "N" };
			Assert.AreEqual("N", formatter4.Formate(str2));
			Assert.AreEqual("很抱歉", formatter4.Formate(str3));

			string str7 = "收货100人啊啊";
			RegexFormatter formatter5 = new RegexFormatter { Pattern = @"收货[\d]+人" };
			Assert.AreEqual("", formatter5.Formate(str2));
			Assert.AreEqual("", formatter5.Formate(str3));
			Assert.AreEqual("收货100人", formatter5.Formate(str7));
		}

		[TestMethod]
		public void CharacterCaseFormatterTest()
		{
			CharacterCaseFormatter formatter1 = new CharacterCaseFormatter();
			string str1 = "";
			Assert.AreEqual(null, formatter1.Formate(str1));
			Assert.AreEqual(null, formatter1.Formate(null));

			string str2 = "a";
			Assert.AreEqual("A", formatter1.Formate(str2));

			CharacterCaseFormatter formatter2 = new CharacterCaseFormatter { ToUpper = false };
			Assert.AreEqual("a", formatter2.Formate(str2));

			string str3 = "A";
			Assert.AreEqual("a", formatter2.Formate(str3));
			Assert.AreEqual(null, formatter2.Formate(null));

			CharacterCaseFormatter formatter3 = new CharacterCaseFormatter { ToUpper = false, ValueWhenNull = "OK" };
			Assert.AreEqual("OK", formatter3.Formate(null));
			Assert.AreEqual("OK", formatter3.Formate(""));
		}

		[TestMethod]
		public void DisplaceFormaterTest()
		{
			DisplaceFormater formatter1 = new DisplaceFormater { Displacement = "d", EqualValue = "a" };
			string str1 = "";
			Assert.AreEqual("", formatter1.Formate(str1));
			Assert.AreEqual("d", formatter1.Formate("a"));
			Assert.AreEqual("dd", formatter1.Formate("dd"));

			//DisplaceFormater formatter2 = new DisplaceFormater { Displacement = 3, EqualValue = 1 };
			//Assert.AreEqual(2, formatter2.Formate(2));
			//Assert.AreEqual(3, formatter2.Formate(1));
			//Assert.AreEqual("dd", formatter2.Formate("dd"));
		}

		[TestMethod]
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
				Assert.AreEqual("FormatString should not be null or empty.", se.Message);
			}
			try
			{
				var f = new FormatStringFormater { Format = null };
				f.Formate("");
				throw new Exception("TEST FAILED.");
			}
			catch (SpiderException se)
			{
				Assert.AreEqual("FormatString should not be null or empty.", se.Message);
			}
			try
			{
				var f = new FormatStringFormater { Format = "     " };
				f.Formate("");
				throw new Exception("TEST FAILED.");
			}
			catch (SpiderException se)
			{
				Assert.AreEqual("FormatString should not be null or empty.", se.Message);
			}

			FormatStringFormater formatter1 = new FormatStringFormater { Format = "http://{0}" };
			Assert.AreEqual("http://a", formatter1.Formate("a"));

			FormatStringFormater formatter2 = new FormatStringFormater { Format = "http://{0}/{1}" };
			Assert.AreEqual("http://a/b", formatter2.Formate(new[] { "a", "b" }));
		}
	}
}
