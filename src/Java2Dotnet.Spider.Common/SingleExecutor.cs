using System;
using System.Threading;

namespace Java2Dotnet.Spider.Common
{
	public class SingleExecutor
	{
		private string _locker = "unlock";

		public void Execute(Action action)
		{
			if (_locker == "lock")
			{
				while (true)
				{
					Thread.Sleep(50);
					if (_locker != "lock")
					{
						return;
					}
				}
			}
			else
			{
				try
				{
					_locker = "lock";
					action();
				}
				finally
				{
					_locker = "unlock";
				}
			}
		}
	}
}
