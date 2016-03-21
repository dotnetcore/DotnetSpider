using System.Text.RegularExpressions;
using Java2Dotnet.Spider.Core;

namespace Java2Dotnet.Spider.Extension.Model.Formatter
{
	public sealed class RegexFormater : CustomizeFormatter
	{
		protected override dynamic FormatTrimmed(string raw)
		{
			if (Extra == null || Extra.Length != 2)
			{
				throw new SpiderExceptoin("RegexFormater need 2 parameters.");
			}

			int group;
			if (!int.TryParse(Extra[1], out group))
			{
				throw new SpiderExceptoin("The second parameter of RegexFormater is INT.");
			}

			Regex regex;
			try
			{
				regex = new Regex(Extra[0]);
			}
			catch
			{
				throw new SpiderExceptoin("RegexFormater: 正则式不合法");
			}

			return regex.Match(raw).Groups[int.Parse(Extra[1])].Value;
		}
	}
}
