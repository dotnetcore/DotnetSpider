using System;
using HtmlAgilityPack;

namespace DotnetSpider.Extension.Model.Formatter
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public class RemoveHtmlTagFormater : Formatter
	{
		protected override dynamic FormateValue(dynamic value)
		{
			string tmp = value.ToString();
			var htmlDocument = new HtmlDocument();
			htmlDocument.LoadHtml(tmp);
			return htmlDocument.DocumentNode.InnerText;
		}

		protected override void CheckArguments()
		{
		}
	}
}
