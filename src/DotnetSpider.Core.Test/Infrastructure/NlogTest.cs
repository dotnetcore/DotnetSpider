using DotnetSpider.Core.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Xml;

namespace DotnetSpider.Core.Test.Infrastructure
{
	[TestClass]
	public class NlogTest
	{
		[TestMethod]
		public void WithoutNlogConfig()
		{
			var nlogConfig = LogCenter.GetDefaultConfigString();
			XmlDocument document = new XmlDocument();
			document.Load(new StringReader(nlogConfig));
			Assert.AreEqual("nlog", document.DocumentElement.Name);
			Assert.AreEqual(2, document.DocumentElement.ChildNodes.Count);
			Assert.AreEqual(2, document.DocumentElement.ChildNodes[0].ChildNodes.Count);
			Assert.AreEqual("target", document.DocumentElement.ChildNodes[0].ChildNodes[0].Name);
			Assert.AreEqual("console", document.DocumentElement.ChildNodes[0].ChildNodes[0].Attributes["name"].Value);
			Assert.AreEqual("file", document.DocumentElement.ChildNodes[0].ChildNodes[1].Attributes["name"].Value);
			Assert.AreEqual(2, document.DocumentElement.ChildNodes[1].ChildNodes.Count);
			Assert.AreEqual("logger", document.DocumentElement.ChildNodes[1].ChildNodes[0].Name);
			Assert.AreEqual("console", document.DocumentElement.ChildNodes[1].ChildNodes[0].Attributes["writeTo"].Value);
			Assert.AreEqual("file", document.DocumentElement.ChildNodes[1].ChildNodes[1].Attributes["writeTo"].Value);
		}
	}
}
