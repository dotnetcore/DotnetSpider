using System;

namespace DotnetSpider.Core
{
	public interface IControllable 
	{
		void Pause(Action action = null);
		void Contiune();
		void Exit(Action action = null);
	}
}
