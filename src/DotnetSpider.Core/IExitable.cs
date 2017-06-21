using System;

namespace DotnetSpider.Core
{
	public interface IExitable
	{
		void Exit(Action action = null);
	}
}
