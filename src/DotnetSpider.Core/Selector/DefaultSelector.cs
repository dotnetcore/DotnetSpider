using System.Collections.Generic;

namespace DotnetSpider.Core.Selector
{
	internal class DefaultSelector : ISelector
	{
		public dynamic Select(dynamic text)
		{
			return null;
		}

		public IEnumerable<dynamic> SelectList(dynamic text)
		{
			return null;
		}
	}
}
