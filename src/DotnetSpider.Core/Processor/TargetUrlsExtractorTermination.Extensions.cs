using System.Linq;

namespace DotnetSpider.Core.Processor
{
	public class TerminateWhenPageContains : ITargetUrlsExtractorTermination
	{
		private readonly string[] _contents;

		public TerminateWhenPageContains(string[] contents)
		{
			_contents = contents;
		}

		public bool IsTermination(Page page)
		{
			if (string.IsNullOrEmpty(page?.Content))
			{
				return false;
			}

			return _contents.Any(c => page.Content.Contains(c));
		}
	}

	public class TerminateWhenPageUnContains : ITargetUrlsExtractorTermination
	{
		private readonly string[] _contents;

		public TerminateWhenPageUnContains(string[] contents)
		{
			_contents = contents;
		}

		public bool IsTermination(Page page)
		{
			if (string.IsNullOrEmpty(page?.Content))
			{
				return false;
			}

			return !_contents.All(c => page.Content.Contains(c));
		}
	}
}
