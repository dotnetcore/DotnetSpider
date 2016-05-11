using Java2Dotnet.Spider.Core;
using Java2Dotnet.Spider.Core.Selector;
#if !NET_CORE
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace Java2Dotnet.Spider.Test
{
	[TestClass]
	public class HtmlTest
	{
		[TestMethod]
		public void TestRegexSelector()
		{
			Selectable selectable = new Selectable("aaaaaaab", "", ContentType.Html);
			//        Assert.assertEquals("abbabbab", (selectable.regex("(.*)").replace("aa(a)", "$1bb").toString()));
			string value = selectable.Regex("(.*)").GetValue();
			Assert.AreEqual("aaaaaaab", value);

		}
	}
}
