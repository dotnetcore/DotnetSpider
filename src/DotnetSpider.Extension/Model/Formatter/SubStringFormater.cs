using System;

namespace DotnetSpider.Extension.Model.Formatter
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public class SubStringFormatter : Formatter
	{
	    public int Start { get; set; } = 0;

	    public int Length { get; set; } = 0;

        public string FindFirstIndex { get; set; }
        public string FindLastIndex { get; set; }
        protected override object FormateValue(object value)
		{
			var tmp = value.ToString().Trim();
		    var firstIndex = 0;
		    var lastIndex = 0;
		    if (!string.IsNullOrEmpty(FindFirstIndex))
		        firstIndex = tmp.IndexOf(FindFirstIndex );
		    if (!string.IsNullOrEmpty(FindLastIndex))
		        lastIndex = tmp.LastIndexOf(FindLastIndex );

		    var start = Start;
		    var length = Length;
            if (firstIndex > 0)
		    {
		        length += firstIndex;
		    }
		    if (lastIndex > 0)
		    {
		        start += lastIndex;
		    }
            if (length <= 0)
		    {
		        length += tmp.Length- start;
		    }



			return tmp.Substring(start, length);
		}

		protected override void CheckArguments()
		{
		}
	}
}
