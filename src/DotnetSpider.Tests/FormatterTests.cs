using System;
using DotnetSpider.DataFlow.Parser.Formatters;
using Xunit;

namespace DotnetSpider.Tests
{
    public class FormatterTests
    {
        [Fact]
        public void RegexFormatter()
        {
            try
            {
                var f = new RegexFormatter {False = "F", Default = "Y", Pattern = "  ", True = "Y"};
                f.Format("");
                throw new Exception("TEST FAILED.");
            }
            catch (ArgumentException se)
            {
                Assert.Equal("Pattern should not be null or empty", se.Message);
            }

            try
            {
                var f = new RegexFormatter {False = "F", Default = "Y", Pattern = "", True = "Y"};
                f.Format("");
                throw new Exception("TEST FAILED.");
            }
            catch (ArgumentException se)
            {
                Assert.Equal("Pattern should not be null or empty", se.Message);
            }

            try
            {
                var f = new RegexFormatter {False = "F", Default = "Y", Pattern = null, True = "Y"};
                f.Format("");
                throw new Exception("TEST FAILED.");
            }
            catch (ArgumentException se)
            {
                Assert.Equal("Pattern should not be null or empty", se.Message);
            }

            var formatter1 = new RegexFormatter {False = "F", Default = "Y", Pattern = "很抱歉", True = "Y"};
            var str1 = "";
            Assert.Equal("F", formatter1.Format(str1));

            var formatter2 = new RegexFormatter {False = "F", Default = "F", Pattern = "很抱歉", True = "Y"};
            Assert.Equal("F", formatter2.Format(str1));

            var str2 = "ABCD";
            Assert.Equal("F", formatter2.Format(str2));
            Assert.Equal("F", formatter1.Format(str2));

            var str3 = "ABCD很抱歉";
            Assert.Equal("Y", formatter2.Format(str3));
            Assert.Equal("Y", formatter1.Format(str3));

            var str4 = "很抱歉ABCD";
            Assert.Equal("Y", formatter2.Format(str4));
            Assert.Equal("Y", formatter1.Format(str4));

            var str5 = "ABCD很抱歉ABCD";
            Assert.Equal("Y", formatter2.Format(str5));
            Assert.Equal("Y", formatter1.Format(str5));

            var str6 = "       很抱歉ABCD";
            Assert.Equal("Y", formatter2.Format(str6));
            Assert.Equal("Y", formatter1.Format(str6));

            var formatter3 = new RegexFormatter {Pattern = "很抱歉", True = "Y"};
            Assert.Equal("", formatter3.Format(str2));
            Assert.Equal("Y", formatter3.Format(str3));

            var formatter4 = new RegexFormatter {Pattern = "很抱歉", False = "N"};
            Assert.Equal("N", formatter4.Format(str2));
            Assert.Equal("很抱歉", formatter4.Format(str3));

            var str7 = "收货100人啊啊";
            var formatter5 = new RegexFormatter {Pattern = @"收货[\d]+人"};
            Assert.Equal("", formatter5.Format(str2));
            Assert.Equal("", formatter5.Format(str3));
            Assert.Equal("收货100人", formatter5.Format(str7));
        }

        [Fact]
        public void CharacterCaseFormatter()
        {
            var formatter1 = new CharacterCaseFormatter();
            var str1 = "";
            Assert.Equal("", formatter1.Format(str1));
            Assert.Null(formatter1.Format(null));

            var str2 = "a";
            Assert.Equal("A", formatter1.Format(str2));

            var formatter2 = new CharacterCaseFormatter {ToUpper = false};
            Assert.Equal("a", formatter2.Format(str2));

            var str3 = "A";
            Assert.Equal("a", formatter2.Format(str3));
            Assert.Null(formatter2.Format(null));

            var formatter3 = new CharacterCaseFormatter {ToUpper = false, Default = "OK"};
            Assert.Equal("OK", formatter3.Format(null));
            Assert.Equal("", formatter3.Format(""));
        }

        [Fact]
        public void DisplaceFormatter()
        {
            var formatter1 = new DisplaceFormatter {Displacement = "d", EqualValue = "a"};
            var str1 = "";
            Assert.Equal("", formatter1.Format(str1));
            Assert.Equal("d", formatter1.Format("a"));
            Assert.Equal("dd", formatter1.Format("dd"));

            //DisplaceFormater formatter2 = new DisplaceFormater { Displacement = 3, EqualValue = 1 };
            //Assert.Equal(2, formatter2.Formate(2));
            //Assert.Equal(3, formatter2.Formate(1));
            //Assert.Equal("dd", formatter2.Formate("dd"));
        }

        [Fact]
        public void FormatStringFormatter()
        {
            try
            {
                var f = new StringFormatter {FormatStr = ""};
                f.Format("");
                throw new Exception("TEST FAILED.");
            }
            catch (ArgumentException se)
            {
                Assert.Equal("FormatString should not be null or empty", se.Message);
            }

            try
            {
                var f = new StringFormatter {FormatStr = null};
                f.Format("");
                throw new Exception("TEST FAILED.");
            }
            catch (ArgumentException se)
            {
                Assert.Equal("FormatString should not be null or empty", se.Message);
            }

            try
            {
                var f = new StringFormatter {FormatStr = "     "};
                f.Format("");
                throw new Exception("TEST FAILED.");
            }
            catch (ArgumentException se)
            {
                Assert.Equal("FormatString should not be null or empty", se.Message);
            }

            var formatter1 = new StringFormatter {FormatStr = "http://{0}"};
            Assert.Equal("http://a", formatter1.Format("a"));

            //StringFormater formatter2 = new StringFormater { Format = "http://{0}/{1}" };
            //Assert.Equal("http://a/b", formatter2.Formate(new[] { "a", "b" }));
        }
    }
}