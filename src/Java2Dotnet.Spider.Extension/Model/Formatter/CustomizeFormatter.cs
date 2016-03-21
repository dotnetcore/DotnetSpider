using HtmlAgilityPack;
using Java2Dotnet.Spider.Core;

namespace Java2Dotnet.Spider.Extension.Model.Formatter
{
	public abstract class CustomizeFormatter : IObjectFormatter
	{
		public IObjectFormatter NextFormatter { get; set; }

		public string[] Extra { get; set; }

		public Page Page { get; set; }

		public void InitParam(string[] extra)
		{
			Extra = extra;
		}

		public virtual dynamic Format(string raw)
		{
			if (string.IsNullOrEmpty(raw))
			{
				return null;
			}

			HtmlDocument document = new HtmlDocument();
			document.LoadHtml(raw);
			if (NextFormatter != null)
			{
				return NextFormatter.Format(FormatTrimmed(document.DocumentNode.InnerText));
			}
			return FormatTrimmed(document.DocumentNode.InnerText);
		}

		protected abstract dynamic FormatTrimmed(string raw);
	}
}
