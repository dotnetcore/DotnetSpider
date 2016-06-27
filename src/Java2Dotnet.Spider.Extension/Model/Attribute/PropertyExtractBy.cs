using System;

namespace Java2Dotnet.Spider.Extension.Model.Attribute
{
	
	[AttributeUsage(AttributeTargets.Property)]
	public class PropertyExtractBy : BaseExtractBy
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
		public bool NotNull { get; set; }

		public ValueOption Option { get; set; } = ValueOption.None;

		//public bool Multi { get; set; } = false;
	}
}
