using DotnetSpider.Core.Infrastructure;
using Xunit;
using System.IO;
using System.Xml;

namespace DotnetSpider.Core.Test.Infrastructure
{

	public class NlogTest
	{
		[Fact]
		public void WithoutNlogConfig()
		{
			var nlogConfig = DLog.GetDefaultConfigString();
			XmlDocument document = new XmlDocument();
			document.Load(new StringReader(nlogConfig));
			Assert.Equal("nlog", document.DocumentElement.Name);
			Assert.Equal(2, document.DocumentElement.ChildNodes.Count);
			Assert.Equal(2, document.DocumentElement.ChildNodes[0].ChildNodes.Count);
			Assert.Equal("target", document.DocumentElement.ChildNodes[0].ChildNodes[0].Name);
			Assert.Equal("console", document.DocumentElement.ChildNodes[0].ChildNodes[0].Attributes["name"].Value);
			Assert.Equal("file", document.DocumentElement.ChildNodes[0].ChildNodes[1].Attributes["name"].Value);
			Assert.Equal(2, document.DocumentElement.ChildNodes[1].ChildNodes.Count);
			Assert.Equal("logger", document.DocumentElement.ChildNodes[1].ChildNodes[0].Name);
			Assert.Equal("console", document.DocumentElement.ChildNodes[1].ChildNodes[0].Attributes["writeTo"].Value);
			Assert.Equal("file", document.DocumentElement.ChildNodes[1].ChildNodes[1].Attributes["writeTo"].Value);
		}
	}
}
