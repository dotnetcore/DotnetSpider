using System;
using HtmlAgilityPack;

namespace DotnetSpider.Extension.Model.Formatter
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public class RemoveHtmlTagFormater : Formatter
	{
		protected override dynamic FormateValue(dynamic value)
		{
			var htmlDocument = new HtmlDocument();
			htmlDocument.LoadHtml(value);
			return htmlDocument.DocumentNode.InnerText;
		}

		protected override void CheckArguments()
		{
		}
	}
}
