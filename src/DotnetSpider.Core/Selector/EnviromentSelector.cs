using System.Collections.Generic;

namespace DotnetSpider.Core.Selector
{
	public class EnviromentSelector : ISelector
	{
		public string Field { get; }

		public EnviromentSelector(string field)
		{
			Field = field;
		}

		public dynamic Select(dynamic text)
		{
			throw new SpiderException("EnviromentSelector does not support SelectList method now.");
		}

		public List<dynamic> SelectList(dynamic text)
		{
			throw new SpiderException("EnviromentSelector does not support SelectList method now.");
		}
	}
}
