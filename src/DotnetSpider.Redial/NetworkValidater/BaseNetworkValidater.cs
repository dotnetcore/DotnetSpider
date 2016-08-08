using System.Threading;

namespace DotnetSpider.Redial.NetworkValidater
{
	public abstract class BaseNetworkValidater : INetworkValidater
	{
		public int MaxWaitTime { get; set; } = 10;

		public bool Wait()
		{
			int currentWaitTime = 0;
			while (currentWaitTime < MaxWaitTime)
			{
				currentWaitTime++;
				try
				{
					if (DoValidate())
					{
						return true;
					}
					Thread.Sleep(2000);
				}
				catch
				{
					// ignored
				}
			}
			return false;
		}

		public abstract bool DoValidate();
	}
}
