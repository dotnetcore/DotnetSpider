using DotnetSpider.Core.Selector;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
	}
}
