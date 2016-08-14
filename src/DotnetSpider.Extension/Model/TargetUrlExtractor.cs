using System.Collections.Generic;

namespace DotnetSpider.Extension.Model
{
	public class TargetUrlExtractor
	{
		public List<string> Patterns { get; set; } = new List<string>();
		public List<Formatter.Formatter> Formatters { get; set; }
		public Selector Region { get; set; }
        public Dictionary<string, dynamic> Extras { get; set; } = new Dictionary<string, dynamic>();

        public TargetUrlExtractor Clone()
        {
            var result = new TargetUrlExtractor();
            foreach (var item in Patterns)
            {
                result.Patterns.Add(item);
            }
            if (Formatters != null)
            {
                result.Formatters = new List<Formatter.Formatter>();
                foreach (var item in Formatters)
                {
                    result.Formatters.Add(item);
                }
            }
            result.Region = Region;
            foreach (var item in Extras)
            {
                result.Extras.Add(item.Key, item.Value);
            }
            return result;
        }
    }
}
