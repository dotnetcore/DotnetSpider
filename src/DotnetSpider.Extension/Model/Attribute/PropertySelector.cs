using System;

namespace DotnetSpider.Extension.Model.Attribute
{
	[AttributeUsage(AttributeTargets.Property)]
	public class PropertySelector : BaseSelector
	{
		public enum ValueOption
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

		public ValueOption Option { get; set; } = ValueOption.None;
	}
}
