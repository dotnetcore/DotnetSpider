using System;

namespace DotnetSpider.Core
{
	public interface IPausable
	{
		void Pause(Action action = null);
	}
}
