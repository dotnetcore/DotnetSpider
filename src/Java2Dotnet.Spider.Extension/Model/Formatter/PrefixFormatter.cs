using Java2Dotnet.Spider.Core;

namespace Java2Dotnet.Spider.Extension.Model.Formatter
{
	public sealed class PrefixFormatter : CustomizeFormatter
	{
		protected override dynamic FormatTrimmed(string raw )
		{
			if (Extra == null || Extra.Length != 1)
			{
				throw new SpiderExceptoin("PrefixFormatter need 1 parameter.");
			}
			return  Extra[0] + raw; 
		}
	}
}
