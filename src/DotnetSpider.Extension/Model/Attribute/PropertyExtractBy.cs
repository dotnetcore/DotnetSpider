using System;

namespace DotnetSpider.Extension.Model.Attribute
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

        /// <summary>
        /// Get the part matching the pattern from the value extracted.
        /// </summary>
        public string Pattern { get; set; }
        /// <summary>
        /// Work together with "Pattern", generate a new result string by a regex replacing.
        /// </summary>
        public string ReplaceString { get; set; }

    }
}
