using System;

namespace DotnetSpider.Extension.Model.Attribute
{
	[AttributeUsage(AttributeTargets.Property)]
	public class PropertyDefine : BaseSelector
	{
		public enum Options
		{
			None,
			PlainText,
			Count
		}

		/// <summary>
		/// Define whether the field can be null. 
		/// If set to 'true' and the extractor get no result, the entire class will be discarded.
		/// </summary>
		public bool NotNull { get; set; } = false;

		public Options Option { get; set; } = Options.None;

		public int Length { get; set; } = 0;

		public bool IgnoreStore { get; set; } = false;
	}
}
