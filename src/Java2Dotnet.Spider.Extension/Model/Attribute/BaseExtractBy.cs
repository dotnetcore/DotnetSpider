namespace Java2Dotnet.Spider.Extension.Model.Attribute
{
	public class BaseExtractBy : System.Attribute
	{
		/// <summary>
		/// Extractor expression, support XPath, CSS Selector and regex.
		/// </summary>
		public string Expression { get; set; }

		/// <summary>
		/// Extractor type, support XPath, CSS Selector and regex.
		/// </summary>
		public ExtractType Type { get; set; } = ExtractType.XPath;

		public long Count { get; set; } = long.MaxValue;
	}
}
