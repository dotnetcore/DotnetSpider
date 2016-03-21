using Java2Dotnet.Spider.Core;

namespace Java2Dotnet.Spider.Extension.Model.Formatter
{
	public sealed class AppendFormatter : CustomizeFormatter
	{
		protected override dynamic FormatTrimmed(string raw)
		{
			if (Extra == null || Extra.Length != 1)
			{
				throw new SpiderExceptoin("AppendFormatter need 1 parameter.");
			}

			if (raw == null)
			{
				raw = "";
			}

			return raw + "Extra[0]";
		}
	}
}
